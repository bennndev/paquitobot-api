using MediatR;
using PaquitoBot.Application.UseCases.SyncRuns.Command;
using PaquitoBot.Application.UseCases.SyncRuns.Query;
using PaquitoBot.Domain.Constants;

namespace PaquitoBot.Application.Tests.Fakes;

/// <summary>Plays the SyncRun use cases in memory so orchestration can be tested without a database.</summary>
public class FakeSender : ISender
{
    public List<GetAllSyncRunsResponse> ExistingRuns { get; } = [];

    public List<StartSyncRunCommand> Started { get; } = [];

    public List<(Guid Id, CompleteSyncRunCommand Command)> Completed { get; } = [];

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        object result = request switch
        {
            GetAllSyncRunsQuery query => (IReadOnlyList<GetAllSyncRunsResponse>)ExistingRuns
                .Where(run => query.Status is null || run.Status == query.Status)
                .ToList(),
            StartSyncRunCommand command => RecordStart(command),
            _ => throw new NotSupportedException(request.GetType().Name)
        };

        return Task.FromResult((TResponse)result);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        if (request is CompleteSyncRunCommand command)
        {
            Completed.Add((command.SyncRunId, command));
            return Task.CompletedTask;
        }

        throw new NotSupportedException(typeof(TRequest).Name);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    private Guid RecordStart(StartSyncRunCommand command)
    {
        Started.Add(command);

        return Guid.NewGuid();
    }

    public static GetAllSyncRunsResponse RunningSince(DateTime startDate)
        => new(Guid.NewGuid(), "Student", SyncRunStatus.Running, startDate, null, 0, null);
}
