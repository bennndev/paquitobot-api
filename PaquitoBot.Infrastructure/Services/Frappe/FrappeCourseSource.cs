using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeCourseSource(FrappeApiClient client) : IFrappeSource<FrappeCourseDto>
{
    private const string DocType = "Course";

    private static readonly string[] Fields =
        ["name", "course_name", "course_abbreviation", "short_description", "course_credit", "modified"];

    public async Task<IReadOnlyList<FrappeCourseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await client.GetListAsync<FrappeCourse>(DocType, Fields, cancellationToken);

        return courses.Select(course => course.ToDto()).ToList();
    }

    private sealed record FrappeCourse
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("course_name")]
        public string? CourseName { get; init; }

        [JsonPropertyName("course_abbreviation")]
        public string? Abbreviation { get; init; }

        [JsonPropertyName("short_description")]
        public string? ShortDescription { get; init; }

        [JsonPropertyName("course_credit")]
        public double? Credit { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        // Frappe stores credits as a float; PaquitoBot keeps whole credits.
        public FrappeCourseDto ToDto() => new(
            Name, Abbreviation, CourseName ?? Name, ShortDescription,
            Credit is null ? null : (int)Math.Round(Credit.Value), null, FrappeFormats.ParseTimestamp(Modified));
    }
}
