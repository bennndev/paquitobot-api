using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeStudentSource(FrappeApiClient client) : IFrappeSource<FrappeStudentDto>
{
    private const string DocType = "Student";

    private static readonly string[] Fields =
    [
        "name", "student_code", "first_name", "last_name", "student_name", "student_email",
        "student_mobile_number", "date_of_birth", "gender", "enabled", "modified"
    ];

    public async Task<IReadOnlyList<FrappeStudentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var students = await client.GetListAsync<FrappeStudent>(DocType, Fields, cancellationToken);

        return students.Select(student => student.ToDto()).ToList();
    }

    private sealed record FrappeStudent
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("student_code")]
        public string? StudentCode { get; init; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("student_name")]
        public string? StudentName { get; init; }

        [JsonPropertyName("student_email")]
        public string? Email { get; init; }

        [JsonPropertyName("student_mobile_number")]
        public string? Phone { get; init; }

        [JsonPropertyName("date_of_birth")]
        public DateOnly? BirthDate { get; init; }

        [JsonPropertyName("gender")]
        public string? Gender { get; init; }

        [JsonPropertyName("enabled")]
        public int Enabled { get; init; }

        [JsonPropertyName("modified")]
        public DateTime? Modified { get; init; }

        public FrappeStudentDto ToDto() => new(
            Name, StudentCode, FirstName, LastName, StudentName, Email, Phone, BirthDate, Gender,
            Enabled == 1 ? "Active" : "Inactive", Modified);
    }
}
