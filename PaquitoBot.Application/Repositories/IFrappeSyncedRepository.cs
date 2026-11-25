using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IFrappeSyncedRepository<T> : IGenericRepository<T> where T : class, IFrappeSynced
{
    Task<T?> GetByFrappeExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, T>> GetByFrappeExternalIdsAsync(
        IEnumerable<string> externalIds,
        CancellationToken cancellationToken = default);
}
