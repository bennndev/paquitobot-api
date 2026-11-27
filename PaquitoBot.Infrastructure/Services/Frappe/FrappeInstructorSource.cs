using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeInstructorSource(IFrappeClient client) : IFrappeSource<FrappeInstructorDto>
{
    private const string DocType = "Faculty";

    private static readonly string[] Fields =
        ["name", "first_name", "last_name", "faculty_name", "email_address", "phone_number", "status", "modified"];

    public async Task<IReadOnlyList<FrappeInstructorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var faculty = await client.GetListAsync<FrappeFaculty>(DocType, Fields, cancellationToken);

        return faculty.Select(member => member.ToDto()).ToList();
    }

    private sealed record FrappeFaculty
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("faculty_name")]
        public string? FacultyName { get; init; }

        [JsonPropertyName("email_address")]
        public string? Email { get; init; }

        [JsonPropertyName("phone_number")]
        public string? Phone { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        public FrappeInstructorDto ToDto() => new(
            Name, FirstName, LastName, FacultyName, Email, Phone, Status, FrappeFormats.ParseTimestamp(Modified));
    }
}
