namespace PaquitoBot.Domain.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<T> UpsertByFrappeExternalIdAsync(string externalId, Action<T> update,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Remove(T entity);
}
