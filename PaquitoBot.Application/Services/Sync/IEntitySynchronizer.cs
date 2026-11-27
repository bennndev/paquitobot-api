using PaquitoBot.Application.DTOs.Sync;

namespace PaquitoBot.Application.Services.Sync;

public interface IEntitySynchronizer
{
    string EntityName { get; }

    int Order { get; }

    Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
