namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeProgramEnrollmentDto(
    string ExternalId,
    string StudentExternalId,
    string ProgramExternalId,
    string? AcademicPeriodExternalId,
    DateOnly? EnrollmentDate,
    string? Status,
    DateTime? ModifiedAt);
