using PaquitoBot.Application.DTOs.Frappe;

namespace PaquitoBot.Application.Services;

/// <summary>
/// Read-only contract to fetch academic master data from Frappe.
/// Implementations must never expose credentials in exceptions or logs.
/// </summary>
public interface IFrappeClient
{
    Task<IReadOnlyList<FrappeAcademicPeriodDto>> GetAcademicPeriodsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeProgramDto>> GetProgramsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeCourseDto>> GetCoursesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeProgramCourseDto>> GetProgramCoursesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeStudentDto>> GetStudentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeInstructorDto>> GetInstructorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeCourseSectionDto>> GetCourseSectionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeCourseScheduleDto>> GetCourseSchedulesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrappeProgramEnrollmentDto>> GetProgramEnrollmentsAsync(CancellationToken cancellationToken = default);
}
