namespace PaquitoBot.Application.Services.Sync;

public record SyncError(string ExternalId, string Message);

public record SyncResult(
    string EntityName,
    int Created,
    int Updated,
    int Unchanged,
    IReadOnlyList<SyncError> Errors)
{
    public int Failed => Errors.Count;

    public int Processed => Created + Updated + Unchanged + Failed;
}
