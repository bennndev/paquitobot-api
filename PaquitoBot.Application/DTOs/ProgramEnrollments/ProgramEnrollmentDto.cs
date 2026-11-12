using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.ProgramEnrollments;

public record ProgramEnrollmentDto(
    Guid ProgramEnrollmentId,
    string? FrappeExternalId,
    Guid StudentId,
    Guid ProgramId,
    Guid? AcademicPeriodId,
    DateOnly? EnrollmentDate,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ProgramEnrollmentDto From(ProgramEnrollment enrollment)
    {
        return new ProgramEnrollmentDto(
            enrollment.ProgramEnrollmentId,
            enrollment.FrappeExternalId,
            enrollment.StudentId,
            enrollment.ProgramId,
            enrollment.AcademicPeriodId,
            enrollment.EnrollmentDate,
            enrollment.Status,
            enrollment.FrappeModifiedAt,
            enrollment.SyncedAt,
            enrollment.CreatedAt,
            enrollment.UpdatedAt);
    }
}
