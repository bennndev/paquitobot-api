namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeAcademicPeriodDto(
    string ExternalId,
    string? Code,
    string Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Status,
    DateTime? ModifiedAt);
