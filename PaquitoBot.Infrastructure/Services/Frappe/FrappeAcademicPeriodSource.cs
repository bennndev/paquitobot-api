using System.Text.Json.Serialization;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;

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

        public FrappeAcademicPeriodDto ToDto() => new(
            Name,
            string.IsNullOrWhiteSpace(TermName) ? null : TermName,
            string.IsNullOrWhiteSpace(Name) ? TermName ?? string.Empty : Name,
            StartDate, EndDate, null, FrappeFormats.ParseTimestamp(Modified));
    }
}
