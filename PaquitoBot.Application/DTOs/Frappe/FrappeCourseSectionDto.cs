namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeCourseSectionDto(
    string ExternalId,
    string CourseExternalId,
    string AcademicPeriodExternalId,
    string? SectionCode,
    string? InstructorName,
    string? DeliveryMode,
    int? Capacity,
    string? Status,
    DateTime? ModifiedAt);
