using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Sync;

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
    public abstract string EntityName { get; }

    public abstract int Order { get; }

    protected abstract Task<IReadOnlyList<TExternal>> FetchAsync(CancellationToken cancellationToken);

    protected abstract string GetExternalId(TExternal external);

    protected abstract DateTime? GetModifiedAt(TExternal external);

    /// <summary>Builds a new entity, including its external id.</summary>
    protected abstract TEntity Create(TExternal external);

    /// <summary>Copies the mutable fields from the external record onto an existing entity.</summary>
    protected abstract void Apply(TExternal external, TEntity entity);

    public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var items = await FetchAsync(cancellationToken);

        var externalIds = items
            .Select(GetExternalId)
            .Where(id => !string.IsNullOrWhiteSpace(id));

        var known = (await repository.GetByFrappeExternalIdsAsync(externalIds, cancellationToken))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        int created = 0, updated = 0, unchanged = 0;
        var errors = new List<SyncError>();

        foreach (var item in items)
        {
            var externalId = GetExternalId(item);

            try
            {
                if (string.IsNullOrWhiteSpace(externalId))
                {
                    throw new InvalidOperationException("The record has no external id.");
                }

                var modifiedAt = GetModifiedAt(item);

                if (!known.TryGetValue(externalId, out var entity))
                {
                    entity = Create(item);
                    Stamp(entity, modifiedAt);
                    await repository.AddAsync(entity, cancellationToken);
                    known[externalId] = entity;
                    created++;
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

    private static bool HasChanged(TEntity entity, DateTime? externalModifiedAt)
        => externalModifiedAt is null || externalModifiedAt != entity.FrappeModifiedAt;

    private void Stamp(TEntity entity, DateTime? modifiedAt)
    {
        entity.FrappeModifiedAt = modifiedAt;
        entity.SyncedAt = timeProvider.GetUtcNow().UtcDateTime;
    }
}
