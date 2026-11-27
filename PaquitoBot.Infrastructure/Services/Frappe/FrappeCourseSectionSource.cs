using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>
/// Course sections live in the Frappe "Student Batch Name" doctype, which has no academic term.
/// The term is taken from the batch's non-cancelled "Course Enrollment" records, and only when
/// they all agree on a single one; otherwise it is left empty for the synchronizer to deduce.
/// </summary>
public sealed class FrappeCourseSectionSource(IFrappeClient client) : IFrappeSource<FrappeCourseSectionDto>
{
    private const int CancelledDocStatus = 2;

    private static readonly string[] BatchFields =
        ["name", "batch_name", "course", "max_strength", "disabled", "start_date", "end_date", "modified"];

    private static readonly string[] EnrollmentFields = ["student_batch", "academic_term", "docstatus"];

    public async Task<IReadOnlyList<FrappeCourseSectionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var batches = await client.GetListAsync<FrappeBatch>("Student Batch Name", BatchFields, cancellationToken);
        var enrollments = await client.GetListAsync<FrappeEnrollment>("Course Enrollment", EnrollmentFields, cancellationToken);

        var termByBatch = enrollments
            .Where(e => e.DocStatus != CancelledDocStatus
                        && !string.IsNullOrWhiteSpace(e.StudentBatch)
                        && !string.IsNullOrWhiteSpace(e.AcademicTerm))
            .GroupBy(e => e.StudentBatch!)
            .Select(group => (Batch: group.Key, Terms: group.Select(e => e.AcademicTerm!).Distinct().ToList()))
            .Where(entry => entry.Terms.Count == 1)
            .ToDictionary(entry => entry.Batch, entry => entry.Terms[0]);

        return batches
            .Select(batch => batch.ToDto(termByBatch.GetValueOrDefault(batch.Name)))
            .ToList();
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
        public FrappeCourseSectionDto ToDto(string? academicTerm) => new(
            Name, Course ?? string.Empty, StartDate, EndDate, BatchName ?? Name, null, null, MaxStrength,
            Disabled == 1 ? "Inactive" : "Active", FrappeFormats.ParseTimestamp(Modified), academicTerm);
    }

    private sealed record FrappeEnrollment
    {
        [JsonPropertyName("student_batch")]
        public string? StudentBatch { get; init; }

        [JsonPropertyName("academic_term")]
        public string? AcademicTerm { get; init; }

        [JsonPropertyName("docstatus")]
        public int DocStatus { get; init; }
    }
}
