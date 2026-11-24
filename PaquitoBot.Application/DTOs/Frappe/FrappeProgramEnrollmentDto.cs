namespace PaquitoBot.Application.DTOs.Frappe;

/// <summary>
/// ProgramExternalId is null when the source could not resolve the program of the enrollment;
/// the synchronizer reports that record as failed.
/// </summary>
public record FrappeProgramEnrollmentDto(
    string ExternalId,
    string StudentExternalId,
    string? ProgramExternalId,
    string? AcademicPeriodExternalId,
    DateOnly? EnrollmentDate,
    string? Status,
    DateTime? ModifiedAt);
