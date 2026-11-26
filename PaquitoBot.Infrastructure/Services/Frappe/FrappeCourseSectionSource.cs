using System.Text.Json.Serialization;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Frappe;

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
