using System.Text.Json;

namespace PaquitoBot.Application.Services.SyncByStudent;

public interface IFrappeStudentSyncService
{
    Task<JsonElement> FetchSnapshotAsync(string accessToken, CancellationToken cancellationToken);
}
