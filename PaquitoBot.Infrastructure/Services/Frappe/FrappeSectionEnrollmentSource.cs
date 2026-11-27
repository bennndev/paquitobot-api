using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>
/// Student-section enrollments come from the Frappe "Course Enrollment" doctype. Cancelled enrollments and
/// those without a batch are skipped, and a student appears once per batch because that pair is unique locally.
/// </summary>
public sealed class FrappeSectionEnrollmentSource(FrappeCourseEnrollmentCatalog catalog)
    : IFrappeSource<FrappeSectionEnrollmentDto>
{
    private const int CancelledDocStatus = 2;

    public async Task<IReadOnlyList<FrappeSectionEnrollmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var enrollments = await catalog.GetAllAsync(cancellationToken);

        return enrollments
            .Where(e => e.DocStatus != CancelledDocStatus
                        && !string.IsNullOrWhiteSpace(e.Student)
                        && !string.IsNullOrWhiteSpace(e.StudentBatch))
            .DistinctBy(e => (e.Student, e.StudentBatch))
            .Select(e => new FrappeSectionEnrollmentDto(
                e.Name, e.Student!, e.StudentBatch!, e.EnrollmentDate, e.Status,
                FrappeFormats.ParseTimestamp(e.Modified)))
            .ToList();
    }
}
