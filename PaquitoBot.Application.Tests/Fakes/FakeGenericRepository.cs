using System.Linq.Expressions;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.Tests.Fakes;

public class FakeGenericRepository<T> : IGenericRepository<T> where T : class
{
    public List<T> Items { get; } = [];

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

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.AsQueryable().FirstOrDefault(predicate));

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.AsQueryable().Any(predicate));

    public Task<T> UpsertByFrappeExternalIdAsync(string externalId, Action<T> update, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
