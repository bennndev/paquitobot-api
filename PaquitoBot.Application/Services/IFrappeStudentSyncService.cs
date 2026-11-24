namespace PaquitoBot.Application.Services;

public record FrappeSyncSummary(Guid SyncRunId, int ProcessedRecords);

public interface IFrappeStudentSyncService
{
    Task<FrappeSyncSummary> SyncAsync(string accessToken, Guid userId, string email,
        CancellationToken cancellationToken);
}
