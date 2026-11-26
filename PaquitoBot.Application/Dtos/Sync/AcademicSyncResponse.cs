namespace PaquitoBot.Application.Dtos.Sync;

public record AcademicSyncResponse(
    DateTime StartedAt,
    DateTime FinishedAt,
    int Processed,
    int Created,
    int Updated,
    int Unchanged,
    int Failed,
    IReadOnlyList<EntitySyncResponse> Entities);
