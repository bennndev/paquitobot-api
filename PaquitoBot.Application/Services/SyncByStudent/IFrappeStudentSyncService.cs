using System.Text.Json;

namespace PaquitoBot.Application.Services.SyncByStudent;

public record FrappeSyncSummary(Guid SyncRunId, int ProcessedRecords);

public interface IFrappeStudentSyncService
{
    Task<JsonElement> FetchSnapshotAsync(string accessToken, CancellationToken cancellationToken);
}
