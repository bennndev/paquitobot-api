using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.UseCases.SyncRuns.Command;
using PaquitoBot.Application.UseCases.SyncRuns.Query;
using PaquitoBot.Domain.Constants;

namespace PaquitoBot.Application.UseCases.Sync.Command;

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

public record AcademicSyncResponse(
    DateTime StartedAt,
    DateTime FinishedAt,
    int Processed,
    int Created,
    int Updated,
    int Unchanged,
    int Failed,
    IReadOnlyList<EntitySyncResponse> Entities);

public record RunAcademicSyncCommand : IRequest<AcademicSyncResponse>;

/// <summary>
/// Runs every entity synchronizer in dependency order, recording one SyncRun per entity.
/// A failure in one entity never stops the others, and nothing local is ever deleted.
/// </summary>
public class RunAcademicSyncCommandHandler(
    IEnumerable<IEntitySynchronizer> synchronizers,
    ISender sender,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<RunAcademicSyncCommand, AcademicSyncResponse>
{
    /// <summary>A run still marked as running after this long is assumed to have died and stops blocking new syncs.</summary>
    private static readonly TimeSpan StaleRunAfter = TimeSpan.FromHours(1);

    private const int MaxErrorsInRunMessage = 10;
    private const int MaxErrorTextLength = 200;
    private const int MaxExceptionTextLength = 500;

    public async Task<AcademicSyncResponse> Handle(RunAcademicSyncCommand request, CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetUtcNow().UtcDateTime;

        await EnsureNoSyncIsRunningAsync(startedAt, cancellationToken);

        var entities = new List<EntitySyncResponse>();

        foreach (var synchronizer in synchronizers.OrderBy(s => s.Order))
        {
            entities.Add(await RunEntityAsync(synchronizer, cancellationToken));
        }

        return new AcademicSyncResponse(
            startedAt,
            timeProvider.GetUtcNow().UtcDateTime,
            entities.Sum(e => e.Processed),
            entities.Sum(e => e.Created),
            entities.Sum(e => e.Updated),
            entities.Sum(e => e.Unchanged),
            entities.Sum(e => e.Failed),
            entities);
    }

    private async Task EnsureNoSyncIsRunningAsync(DateTime now, CancellationToken cancellationToken)
    {
        var running = await sender.Send(new GetAllSyncRunsQuery(Status: SyncRunStatus.Running), cancellationToken);

        if (running.Any(run => now - run.StartDate < StaleRunAfter))
        {
            throw new ConflictException("A synchronization is already running.");
        }
    }

    private async Task<EntitySyncResponse> RunEntityAsync(IEntitySynchronizer synchronizer, CancellationToken cancellationToken)
    {
        var syncRunId = await sender.Send(new StartSyncRunCommand(synchronizer.EntityName), cancellationToken);

        try
        {
            var result = await synchronizer.SyncAsync(cancellationToken);

            await sender.Send(
                new CompleteSyncRunCommand(
                    syncRunId,
                    result.Failed == 0,
                    result.Processed,
                    result.Failed == 0 ? null : BuildFailureMessage(result)),
                cancellationToken);

            return new EntitySyncResponse(
                synchronizer.EntityName,
                syncRunId,
                result.Failed == 0 ? SyncRunStatus.Completed : SyncRunStatus.Failed,
                result.Processed,
                result.Created,
                result.Updated,
                result.Unchanged,
                result.Failed,
                result.Errors);
        }
        catch (OperationCanceledException)
        {
            unitOfWork.DiscardPendingChanges();
            await CompleteAsFailedAsync(syncRunId, "The synchronization was cancelled.", CancellationToken.None);

            throw;
        }
        catch (Exception ex)
        {
            unitOfWork.DiscardPendingChanges();

            var message = Truncate($"{ex.GetType().Name}: {ex.Message}", MaxExceptionTextLength);
            await CompleteAsFailedAsync(syncRunId, message, cancellationToken);

            return new EntitySyncResponse(
                synchronizer.EntityName, syncRunId, SyncRunStatus.Failed, 0, 0, 0, 0, 0, [new SyncError(string.Empty, message)]);
        }
    }

    private Task CompleteAsFailedAsync(Guid syncRunId, string message, CancellationToken cancellationToken)
        => sender.Send(new CompleteSyncRunCommand(syncRunId, false, 0, message), cancellationToken);

    private static string BuildFailureMessage(SyncResult result)
    {
        var details = string.Join("; ", result.Errors
            .Take(MaxErrorsInRunMessage)
            .Select(error => $"{error.ExternalId}: {Truncate(error.Message, MaxErrorTextLength)}"));

        var hidden = result.Errors.Count - MaxErrorsInRunMessage;
        var more = hidden > 0 ? $"; and {hidden} more" : string.Empty;

        return $"{result.Failed} of {result.Processed} records failed " +
               $"(created={result.Created}, updated={result.Updated}, unchanged={result.Unchanged}, failed={result.Failed}). " +
               $"Errors: {details}{more}";
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
