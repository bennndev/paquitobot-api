namespace PaquitoBot.Application.DTOs.Frappe;

/// <summary>
/// A Frappe batch has no academic term of its own. When its enrollments agree on a single term the source
/// provides it in <see cref="AcademicPeriodExternalId"/>; otherwise the synchronizer deduces the period
/// that contains the batch dates.
/// </summary>
public record FrappeCourseSectionDto(
    string ExternalId,
    string CourseExternalId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? SectionCode,
    string? InstructorName,
    string? DeliveryMode,
    int? Capacity,
    string? Status,
    DateTime? ModifiedAt,
    string? AcademicPeriodExternalId = null);
