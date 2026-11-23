namespace PaquitoBot.Application.Frappe.Dtos;

public record FrappeCourseScheduleDto(
    string ExternalId,
    string SectionExternalId,
    string? InstructorExternalId,
    DateOnly ClassDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Location,
    string? DeliveryMode,
    string? Status,
    DateTime? ModifiedAt);
