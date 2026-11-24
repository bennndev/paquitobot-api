using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Application.Tests.Fakes;

public class FakeEntitySynchronizer(
    string entityName,
    int order,
    List<string> executionLog,
    SyncResult? result = null,
    Exception? failure = null) : IEntitySynchronizer
{
    public string EntityName => entityName;

    public int Order => order;

    public Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        executionLog.Add(entityName);

        if (failure is not null)
        {
            throw failure;
        }

        return Task.FromResult(result ?? new SyncResult(entityName, 1, 0, 0, []));
    }
}
