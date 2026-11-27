using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Common.Frappe;

internal static class FrappeRecordOwnership
{
    public static void RequireLocal(IFrappeSynced record)
    {
        if (record.FrappeExternalId is not null || record.SyncedAt is not null)
            throw new ConflictException("This record is managed by Frappe. Make changes in Frappe and synchronize again.");
    }
}
