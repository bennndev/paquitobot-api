using System.Text.Json.Serialization;
using PaquitoBot.Application.Integrations.Frappe;

namespace PaquitoBot.Infrastructure.Services.Frappe;

// One catalog instance is shared by the admin synchronizers in a request scope.
public sealed class FrappeCourseCatalog(IFrappeClient client)
{
    private static readonly string[] Fields =
        ["name", "course_name", "course_abbreviation", "short_description", "course_credit", "program", "modified"];

    private Task<IReadOnlyList<FrappeCourseRow>>? _courses;

    public Task<IReadOnlyList<FrappeCourseRow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _courses ??= client.GetListAsync<FrappeCourseRow>("Course", Fields, cancellationToken);
    }
}

public sealed record FrappeCourseRow
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

    [JsonPropertyName("program")]
    public string? Program { get; init; }

    [JsonPropertyName("modified")]
    public string? Modified { get; init; }
}
