using PaquitoBot.Application.Dtos.Sync;
namespace PaquitoBot.Application.Services;

public interface IEntitySynchronizer
{
    string EntityName { get; }

    int Order { get; }

    Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
