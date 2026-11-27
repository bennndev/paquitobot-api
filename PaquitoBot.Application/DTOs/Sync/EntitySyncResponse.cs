namespace PaquitoBot.Application.DTOs.Sync;

public record EntitySyncResponse(
    string EntityName,
    Guid SyncRunId,
    string Status,
    int Processed,
    int Created,
    int Updated,
    int Unchanged,
    int Failed,
    IReadOnlyList<SyncError> Errors);

