namespace PaquitoBot.Domain.Models;

/// <summary>
/// Marks entities that mirror a record owned by Frappe and are identified by its external id.
/// </summary>
public interface IFrappeSynced
{
    string? FrappeExternalId { get; }

    DateTime? FrappeModifiedAt { get; set; }

    DateTime? SyncedAt { get; set; }
}
