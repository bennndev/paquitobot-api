using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface ISyncRunRepository : IGenericRepository<SyncRun>
{
    Task<IReadOnlyList<SyncRun>> GetFilteredAsync(
        string? entityName,
        string? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncRun>> GetLatestByEntityAsync(CancellationToken cancellationToken = default);
}
