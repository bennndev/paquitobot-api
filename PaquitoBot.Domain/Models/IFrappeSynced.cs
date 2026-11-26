namespace PaquitoBot.Domain.Models;

public interface IFrappeSynced
{
    string? FrappeExternalId { get; }

    DateTime? FrappeModifiedAt { get; set; }

    DateTime? SyncedAt { get; set; }
}
