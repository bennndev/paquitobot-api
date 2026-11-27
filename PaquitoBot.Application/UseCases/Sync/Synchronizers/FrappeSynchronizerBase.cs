using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.DTOs.Sync;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Synchronizers;

/// <summary>
/// Template for idempotent upserts keyed by the Frappe external id.
/// Concrete synchronizers only fetch and map; matching, change detection and bookkeeping live here.
/// </summary>
public abstract class FrappeSynchronizerBase<TExternal, TEntity>(
    IFrappeSyncedRepository<TEntity> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IEntitySynchronizer
    where TEntity : class, IFrappeSynced
{
    protected IFrappeSyncedRepository<TEntity> Repository { get; } = repository;

    public abstract string EntityName { get; }

    public abstract int Order { get; }

    protected abstract Task<IReadOnlyList<TExternal>> FetchAsync(CancellationToken cancellationToken);

    protected abstract string GetExternalId(TExternal external);

    protected abstract DateTime? GetModifiedAt(TExternal external);

    /// <summary>Builds a new entity, including its external id.</summary>
    protected abstract TEntity Create(TExternal external);

    /// <summary>Copies the mutable fields from the external record onto an existing entity.</summary>
    protected abstract void Apply(TExternal external, TEntity entity);

    /// <summary>
    /// Runs once after fetching and before mapping. Synchronizers with parents load them here
    /// in batch and throw from <see cref="Create"/> or <see cref="Apply"/> when a parent is missing,
    /// so only that record is reported as failed.
    /// </summary>
    protected virtual Task PrepareAsync(IReadOnlyList<TExternal> items, CancellationToken cancellationToken)
        => Task.CompletedTask;

    // A synchronizer may attach a uniquely matched local placeholder to a Frappe id.
    protected virtual Task<TEntity?> FindUnlinkedAsync(TExternal item,
        CancellationToken cancellationToken) => Task.FromResult<TEntity?>(null);

    protected virtual void AttachExternalId(TEntity entity, string externalId)
    {
        throw new InvalidOperationException("This synchronizer cannot attach an external id.");
    }

    public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var items = await FetchAsync(cancellationToken);
        await PrepareAsync(items, cancellationToken);

        var externalIds = items
            .Select(NormalizedExternalId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!);

        var known = (await Repository.GetByFrappeExternalIdsAsync(externalIds, cancellationToken))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        int created = 0, updated = 0, unchanged = 0;
        var errors = new List<SyncError>();

        foreach (var item in items)
        {
            var externalId = NormalizedExternalId(item);

            try
            {
                if (string.IsNullOrEmpty(externalId))
                {
                    throw new InvalidOperationException("The record has no external id.");
                }

                var modifiedAt = GetModifiedAt(item);

                if (!known.TryGetValue(externalId, out var entity))
                {
                    entity = await FindUnlinkedAsync(item, cancellationToken);
                    if (entity is null)
                    {
                        entity = Create(item);
                        await Repository.AddAsync(entity, cancellationToken);
                        created++;
                    }
                    else
                    {
                        Apply(item, entity);
                        AttachExternalId(entity, externalId);
                        updated++;
                    }

                    Stamp(entity, modifiedAt);
                    known[externalId] = entity;
                }
                else if (HasChanged(entity, modifiedAt))
                {
                    Apply(item, entity);
                    Stamp(entity, modifiedAt);
                    updated++;
                }
                else
                {
                    unchanged++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add(new SyncError(externalId ?? string.Empty, ex.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SyncResult(EntityName, created, updated, unchanged, errors);
    }

    private string? NormalizedExternalId(TExternal external) => GetExternalId(external)?.Trim();

    private static bool HasChanged(TEntity entity, DateTime? externalModifiedAt)
        => externalModifiedAt is null || externalModifiedAt != entity.FrappeModifiedAt;

    private void Stamp(TEntity entity, DateTime? modifiedAt)
    {
        entity.FrappeModifiedAt = modifiedAt;
        entity.SyncedAt = timeProvider.GetUtcNow().UtcDateTime;
    }
}
