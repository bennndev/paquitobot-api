using System.Text.Json;

namespace PaquitoBot.Application.Services;

public record FrappeSyncSummary(Guid SyncRunId, int ProcessedRecords);

public interface IFrappeStudentSyncService
{
    Task<JsonElement> FetchSnapshotAsync(string accessToken, CancellationToken cancellationToken);
}
