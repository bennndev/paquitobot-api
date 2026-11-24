namespace PaquitoBot.Application.DTOs.Frappe;

/// <summary>
/// A Frappe batch has no academic term, so it carries its own dates and
/// the synchronizer deduces the period that contains them.
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
    DateTime? ModifiedAt);
