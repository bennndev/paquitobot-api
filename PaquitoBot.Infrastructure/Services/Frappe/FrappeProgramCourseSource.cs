using System.Text.Json.Serialization;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeProgramCourseSource(IFrappeClient client) : IFrappeSource<FrappeProgramCourseDto>
{
    private const string DocType = "Course";

    private static readonly string[] Fields = ["name", "program"];

    public async Task<IReadOnlyList<FrappeProgramCourseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await client.GetListAsync<FrappeCourseLink>(DocType, Fields, cancellationToken);

        return courses
            .Where(course => !string.IsNullOrWhiteSpace(course.Program))
            .Select(course => new FrappeProgramCourseDto(course.Program!, course.Name, null, true))
            .ToList();
    }

    private sealed record FrappeCourseLink
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("program")]
        public string? Program { get; init; }
    }
}
