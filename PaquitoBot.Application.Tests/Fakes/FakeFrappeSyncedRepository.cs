using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Fakes;

public class FakeFrappeSyncedRepository<T> : IFrappeSyncedRepository<T> where T : class, IFrappeSynced
{
    public List<T> Items { get; } = [];

    public Task<T?> GetByFrappeExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(e => e.FrappeExternalId == externalId));

    public Task<IReadOnlyDictionary<string, T>> GetByFrappeExternalIdsAsync(
        IEnumerable<string> externalIds,
        CancellationToken cancellationToken = default)
    {
        var ids = externalIds.ToHashSet();
        IReadOnlyDictionary<string, T> result = Items
            .Where(e => e.FrappeExternalId is not null && ids.Contains(e.FrappeExternalId))
            .ToDictionary(e => e.FrappeExternalId!);

        return Task.FromResult(result);
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<T>>(Items.ToList());

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(T entity) => Items.Remove(entity);
}
