using PaquitoBot.Application.DTOs.Sync;

namespace PaquitoBot.Application.Services.Sync;

public interface IEntitySynchronizer
{
    string EntityName { get; }

    /// <summary>
    /// Position in the sync run. Entities that others depend on must have a lower value.
    /// </summary>
    int Order { get; }

    Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
