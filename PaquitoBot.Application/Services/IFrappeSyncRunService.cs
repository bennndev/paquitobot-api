namespace PaquitoBot.Application.Services;

public interface IFrappeSyncRunService
{
    Task<Guid> StartAsync(string entityName, CancellationToken cancellationToken = default);

    Task CompleteAsync(Guid syncRunId, int processedRecords, CancellationToken cancellationToken = default);

    Task FailAsync(Guid syncRunId, int processedRecords, Exception failure, CancellationToken cancellationToken = default);
}
