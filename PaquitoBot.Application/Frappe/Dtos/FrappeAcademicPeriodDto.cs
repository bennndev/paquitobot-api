namespace PaquitoBot.Application.Frappe.Dtos;

public record FrappeAcademicPeriodDto(
    string ExternalId,
    string? Code,
    string Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Status,
    DateTime? ModifiedAt);
