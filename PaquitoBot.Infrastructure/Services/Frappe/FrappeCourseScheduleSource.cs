using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>Class slots live in the Frappe "Subject Schedule" doctype.</summary>
public sealed class FrappeCourseScheduleSource(FrappeApiClient client) : IFrappeSource<FrappeCourseScheduleDto>
{
    private const string DocType = "Subject Schedule";

    private static readonly string[] Fields =
        ["name", "student_batch", "faculty", "schedule_date", "from_time", "to_time", "room", "modified"];

    public async Task<IReadOnlyList<FrappeCourseScheduleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var slots = await client.GetListAsync<FrappeSlot>(DocType, Fields, cancellationToken);

        return slots.Select(slot => slot.ToDto()).ToList();
    }

    private sealed record FrappeSlot
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("student_batch")]
        public string? StudentBatch { get; init; }

        [JsonPropertyName("faculty")]
        public string? Faculty { get; init; }

        [JsonPropertyName("schedule_date")]
        public DateOnly ScheduleDate { get; init; }

        [JsonPropertyName("from_time")]
        public string? FromTime { get; init; }

        [JsonPropertyName("to_time")]
        public string? ToTime { get; init; }

        [JsonPropertyName("room")]
        public string? Room { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        // Delivery mode and status are not part of a slot in Frappe.
        public FrappeCourseScheduleDto ToDto() => new(
            Name, StudentBatch ?? string.Empty, Faculty, ScheduleDate,
            FrappeFormats.ParseTime(FromTime), FrappeFormats.ParseTime(ToTime), Room, null, null,
            FrappeFormats.ParseTimestamp(Modified));
    }
}
