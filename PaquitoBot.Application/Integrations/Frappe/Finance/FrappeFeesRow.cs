using System.Text.Json.Serialization;

namespace PaquitoBot.Application.Integrations.Frappe.Finance;

// Exactly the fields requested from the Frappe Education "Fees" DocType.
// Do NOT use the total of a Payment Entry as a student payment: it may
// contain allocations to several invoices.
public sealed record FrappeFeesRow
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("student")]
    public string? Student { get; init; }

    [JsonPropertyName("fee_schedule")]
    public string? FeeSchedule { get; init; }

    [JsonPropertyName("fee_structure")]
    public string? FeeStructure { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("grand_total")]
    public decimal? GrandTotal { get; init; }

    [JsonPropertyName("outstanding_amount")]
    public decimal? OutstandingAmount { get; init; }

    [JsonPropertyName("posting_date")]
    public DateOnly? PostingDate { get; init; }

    [JsonPropertyName("due_date")]
    public DateOnly? DueDate { get; init; }

    [JsonPropertyName("modified")]
    public DateTime? Modified { get; init; }

    [JsonPropertyName("docstatus")]
    public int? Docstatus { get; init; }
}
