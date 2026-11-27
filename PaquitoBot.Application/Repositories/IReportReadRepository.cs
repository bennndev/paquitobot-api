using PaquitoBot.Application.DTOs.Reports;

namespace PaquitoBot.Application.Repositories;

public interface IReportReadRepository
{
    Task<IReadOnlyList<EnrollmentReportRowDto>> GetEnrollmentsAsync(
        Guid? programId, Guid? academicPeriodId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceReportRowDto>> GetAttendanceAsync(
        Guid? programId, Guid? academicPeriodId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssessmentReportRowDto>> GetAssessmentsAsync(
        Guid? programId, Guid? academicPeriodId, CancellationToken cancellationToken = default);
}
