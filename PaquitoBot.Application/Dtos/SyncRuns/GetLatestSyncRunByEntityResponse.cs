namespace PaquitoBot.Application.Dtos.SyncRuns;

public record GetLatestSyncRunByEntityResponse(
    Guid SyncRunId,
    string EntityName,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    int ProcessedRecords,
    string? ErrorMessage);
