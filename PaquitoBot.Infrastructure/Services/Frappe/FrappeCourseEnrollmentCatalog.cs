using System.Text.Json.Serialization;
using PaquitoBot.Application.Common.Frappe;
using PaquitoBot.Application.Integrations.Frappe;

namespace PaquitoBot.Infrastructure.Services.Frappe;

// The admin section sources read the same enrollment list during one request.
public sealed class FrappeCourseEnrollmentCatalog(IFrappeClient client)
{
    private static readonly string[] Fields =
        ["name", "student", "student_batch", "academic_term", "enrollment_date", "status", "docstatus", "modified"];

    private Task<IReadOnlyList<FrappeCourseEnrollmentRow>>? _enrollments;

    public Task<IReadOnlyList<FrappeCourseEnrollmentRow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _enrollments ??= FetchAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<FrappeCourseEnrollmentRow>> FetchAsync(CancellationToken cancellationToken)
    {
        var enrollments = await client.GetListAsync<FrappeCourseEnrollmentRow>(
            "Course Enrollment", Fields, cancellationToken);

        return enrollments
            .OrderByDescending(row => FrappeTimestampOrder.ParseOrMinimum(row.Modified))
            .ThenByDescending(row => row.EnrollmentDate)
            .ThenByDescending(row => row.Name, StringComparer.Ordinal)
            .ToList();
    }
}

public sealed record FrappeCourseEnrollmentRow
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("student")]
    public string? Student { get; init; }

    [JsonPropertyName("student_batch")]
    public string? StudentBatch { get; init; }

    [JsonPropertyName("academic_term")]
    public string? AcademicTerm { get; init; }

    [JsonPropertyName("enrollment_date")]
    public DateOnly? EnrollmentDate { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("docstatus")]
    public int DocStatus { get; init; }

    [JsonPropertyName("modified")]
    public string? Modified { get; init; }
}
