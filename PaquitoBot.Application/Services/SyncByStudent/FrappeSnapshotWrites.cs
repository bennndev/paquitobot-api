using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.SyncByStudent;

internal static class FrappeSnapshotWrites
{
    internal static Task<T> Upsert<T>(IGenericRepository<T> repository, string externalId, Action<T> update,
        CancellationToken cancellationToken) where T : class
    {
        return repository.UpsertByFrappeExternalIdAsync(externalId, update, cancellationToken);
    }

    // The admin sync must recheck shared records after the student snapshot changes them.
    internal static void MarkSharedRecord(IFrappeSynced entity, DateTime syncedAt)
    {
        entity.FrappeModifiedAt = null;
        entity.SyncedAt = syncedAt;
    }
}
