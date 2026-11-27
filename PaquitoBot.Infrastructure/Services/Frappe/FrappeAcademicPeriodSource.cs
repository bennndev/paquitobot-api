using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeAcademicPeriodSource(IFrappeClient client) : IFrappeSource<FrappeAcademicPeriodDto>
{
    private const string DocType = "Academic Term";

    private static readonly string[] Fields = ["name", "term_name", "term_start_date", "term_end_date", "modified"];

    public async Task<IReadOnlyList<FrappeAcademicPeriodDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var terms = await client.GetListAsync<FrappeAcademicTerm>(DocType, Fields, cancellationToken);

        return terms.Select(term => term.ToDto()).ToList();
    }

    private sealed record FrappeAcademicTerm
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("term_name")]
        public string? TermName { get; init; }

        [JsonPropertyName("term_start_date")]
        public DateOnly? StartDate { get; init; }

        [JsonPropertyName("term_end_date")]
        public DateOnly? EndDate { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        // Mirrors the shared database convention: Code is the short term_name ("2026-II") and Name is the full
        // Frappe identifier ("2026 (2026-II)"). Frappe has no status for a term, so it stays empty.
        public FrappeAcademicPeriodDto ToDto() => new(
            Name,
            string.IsNullOrWhiteSpace(TermName) ? null : TermName,
            string.IsNullOrWhiteSpace(Name) ? TermName ?? string.Empty : Name,
            StartDate, EndDate, null, FrappeFormats.ParseTimestamp(Modified));
    }
}
