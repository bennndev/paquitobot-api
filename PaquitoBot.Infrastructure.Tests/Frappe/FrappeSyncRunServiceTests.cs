using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.UseCases.SyncRuns.Command;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.Frappe;

public sealed class FrappeSyncRunServiceTests
{
    [Fact]
    public async Task StartAndCompleteReuseCommandsInIndependentScopes()
    {
        await using var services = BuildServices();
        await using var outerScope = services.CreateAsyncScope();
        var sync = outerScope.ServiceProvider.GetRequiredService<IFrappeSyncRunService>();
        var outerMarker = outerScope.ServiceProvider.GetRequiredService<ScopeMarker>();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        var id = await sync.StartAsync("Resource", caller.Token);
        await sync.CompleteAsync(id, 4, caller.Token);

        var log = services.GetRequiredService<CommandLog>();
        var start = Assert.IsType<ObservedStart>(log.Start);
        var finish = Assert.IsType<ObservedFinish>(log.Finish);
        Assert.Equal(log.RunId, id);
        Assert.Equal(new StartSyncRunCommand("Resource"), start.Command);
        Assert.Equal(new CompleteSyncRunCommand(id, true, 4), finish.Command);
        Assert.NotSame(outerMarker, start.Scope);
        Assert.NotSame(outerMarker, finish.Scope);
        Assert.NotSame(start.Scope, finish.Scope);
        Assert.Equal(caller.Token, start.Token);
        Assert.Equal(caller.Token, finish.Token);
    }

    [Theory]
    [InlineData(false, "Synchronization failed.")]
    [InlineData(true, "Frappe is rate limiting requests.")]
    public async Task FailRecordsOnlyAnApprovedMessage(bool knownFrappeError, string expectedMessage)
    {
        await using var services = BuildServices();
        await using var scope = services.CreateAsyncScope();
        var sync = scope.ServiceProvider.GetRequiredService<IFrappeSyncRunService>();
        var id = Guid.NewGuid();
        var failure = knownFrappeError
            ? (Exception)new FrappeException(FrappeFailure.RateLimited)
            : new InvalidOperationException("sensitive-upstream-value");

        await sync.FailAsync(id, 2, failure, TestContext.Current.CancellationToken);

        var finish = Assert.IsType<ObservedFinish>(services.GetRequiredService<CommandLog>().Finish);
        Assert.Equal(new CompleteSyncRunCommand(id, false, 2, expectedMessage), finish.Command);
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<CommandLog>();
        services.AddScoped<ScopeMarker>();
        services.AddTransient<IRequestHandler<StartSyncRunCommand, Guid>, RecordingStartHandler>();
        services.AddTransient<IRequestHandler<CompleteSyncRunCommand>, RecordingFinishHandler>();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private sealed class ScopeMarker;

    private sealed record ObservedStart(StartSyncRunCommand Command, ScopeMarker Scope, CancellationToken Token);
    private sealed record ObservedFinish(CompleteSyncRunCommand Command, ScopeMarker Scope, CancellationToken Token);

    private sealed class CommandLog
    {
        public Guid RunId { get; } = Guid.NewGuid();
        public ObservedStart? Start { get; set; }
        public ObservedFinish? Finish { get; set; }
    }

    private sealed class RecordingStartHandler(CommandLog log, ScopeMarker scope)
        : IRequestHandler<StartSyncRunCommand, Guid>
    {
        public Task<Guid> Handle(StartSyncRunCommand request, CancellationToken cancellationToken)
        {
            log.Start = new ObservedStart(request, scope, cancellationToken);
            return Task.FromResult(log.RunId);
        }
    }

    private sealed class RecordingFinishHandler(CommandLog log, ScopeMarker scope)
        : IRequestHandler<CompleteSyncRunCommand>
    {
        public Task Handle(CompleteSyncRunCommand request, CancellationToken cancellationToken)
        {
            log.Finish = new ObservedFinish(request, scope, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
