using PaquitoBot.Application.Dtos.Frappe;

namespace PaquitoBot.Application.Services;

public interface IFrappeSnapshotPersister
{
    Task<FrappeSyncSummary> PersistAsync(string accessToken, Guid userId, string email, CancellationToken cancellationToken);
}
