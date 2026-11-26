using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.UseCases.SyncRuns.Commands;

namespace PaquitoBot.Infrastructure.Services.Frappe;

internal sealed class FrappeSyncRunService(IServiceScopeFactory scopeFactory) : IFrappeSyncRunService
{
    public async Task<Guid> StartAsync(string entityName, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(new StartSyncRunCommand(entityName), cancellationToken);
    }

    public Task CompleteAsync(Guid syncRunId, int processedRecords, CancellationToken cancellationToken = default) =>
        FinishAsync(new CompleteSyncRunCommand(syncRunId, true, processedRecords), cancellationToken);

    public Task FailAsync(
        Guid syncRunId,
        int processedRecords,
        Exception failure,
        CancellationToken cancellationToken = default)
    {
        var safeMessage = failure switch
        {
            FrappeException frappeFailure => frappeFailure.Message,
            OperationCanceledException => "Synchronization was canceled.",
            _ => "Synchronization failed."
        };

        return FinishAsync(new CompleteSyncRunCommand(syncRunId, false, processedRecords, safeMessage),
            cancellationToken);
    }

    private async Task FinishAsync(CompleteSyncRunCommand command, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(command, cancellationToken);
    }
}
