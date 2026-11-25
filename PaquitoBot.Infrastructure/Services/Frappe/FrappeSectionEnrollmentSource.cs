using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>
/// Student-section enrollments come from the Frappe "Course Enrollment" doctype. Cancelled enrollments and
/// those without a batch are skipped, and a student appears once per batch because that pair is unique locally.
/// </summary>
public sealed class FrappeSectionEnrollmentSource(FrappeApiClient client) : IFrappeSource<FrappeSectionEnrollmentDto>
{
    private const int CancelledDocStatus = 2;

    private static readonly string[] Fields =
        ["name", "student", "student_batch", "enrollment_date", "status", "docstatus", "modified"];

    public async Task<IReadOnlyList<FrappeSectionEnrollmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var enrollments = await client.GetListAsync<FrappeEnrollment>("Course Enrollment", Fields, cancellationToken);

        return enrollments
            .Where(e => e.DocStatus != CancelledDocStatus
                        && !string.IsNullOrWhiteSpace(e.Student)
                        && !string.IsNullOrWhiteSpace(e.StudentBatch))
            .DistinctBy(e => (e.Student, e.StudentBatch))
            .Select(e => e.ToDto())
            .ToList();
    }

    private sealed record FrappeEnrollment
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("student")]
        public string? Student { get; init; }

        [JsonPropertyName("student_batch")]
        public string? StudentBatch { get; init; }

        [JsonPropertyName("enrollment_date")]
        public DateOnly? EnrollmentDate { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("docstatus")]
        public int DocStatus { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        public FrappeSectionEnrollmentDto ToDto() => new(
            Name, Student!, StudentBatch!, EnrollmentDate, Status, FrappeFormats.ParseTimestamp(Modified));
    }
}
