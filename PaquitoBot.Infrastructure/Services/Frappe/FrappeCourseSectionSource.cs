using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>Course sections live in the Frappe "Student Batch Name" doctype.</summary>
public sealed class FrappeCourseSectionSource(FrappeApiClient client) : IFrappeSource<FrappeCourseSectionDto>
{
    private const string DocType = "Student Batch Name";

    private static readonly string[] Fields =
        ["name", "batch_name", "course", "max_strength", "disabled", "start_date", "end_date", "modified"];

    public async Task<IReadOnlyList<FrappeCourseSectionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var batches = await client.GetListAsync<FrappeBatch>(DocType, Fields, cancellationToken);

        return batches.Select(batch => batch.ToDto()).ToList();
    }

    private sealed record FrappeBatch
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("batch_name")]
        public string? BatchName { get; init; }

        [JsonPropertyName("course")]
        public string? Course { get; init; }

        [JsonPropertyName("max_strength")]
        public int? MaxStrength { get; init; }

        [JsonPropertyName("disabled")]
        public int Disabled { get; init; }

        [JsonPropertyName("start_date")]
        public DateOnly? StartDate { get; init; }

        [JsonPropertyName("end_date")]
        public DateOnly? EndDate { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        // Instructor and delivery mode are not part of a batch in Frappe.
        public FrappeCourseSectionDto ToDto() => new(
            Name, Course ?? string.Empty, StartDate, EndDate, BatchName ?? Name, null, null, MaxStrength,
            Disabled == 1 ? "Inactive" : "Active", FrappeFormats.ParseTimestamp(Modified));
    }
}
