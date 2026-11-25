namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeSectionEnrollmentDto(
    string ExternalId,
    string StudentExternalId,
    string SectionExternalId,
    DateOnly? EnrollmentDate,
    string? Status,
    DateTime? ModifiedAt);
