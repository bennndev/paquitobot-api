namespace PaquitoBot.Application.Dtos.Frappe;

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
