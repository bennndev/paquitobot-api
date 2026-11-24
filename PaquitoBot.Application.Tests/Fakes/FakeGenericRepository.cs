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
}
