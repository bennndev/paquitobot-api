using PaquitoBot.Application.Frappe;
using PaquitoBot.Application.Frappe.Dtos;

namespace PaquitoBot.Application.Tests.Fakes;

public class FakeFrappeClient : IFrappeClient
{
    public List<FrappeAcademicPeriodDto> AcademicPeriods { get; } = [];

    public List<FrappeProgramDto> Programs { get; } = [];

    public List<FrappeCourseDto> Courses { get; } = [];

    public List<FrappeProgramCourseDto> ProgramCourses { get; } = [];

    public List<FrappeStudentDto> Students { get; } = [];

    public List<FrappeInstructorDto> Instructors { get; } = [];

    public List<FrappeCourseSectionDto> CourseSections { get; } = [];

    public List<FrappeCourseScheduleDto> CourseSchedules { get; } = [];

    public List<FrappeProgramEnrollmentDto> ProgramEnrollments { get; } = [];

    public Task<IReadOnlyList<FrappeAcademicPeriodDto>> GetAcademicPeriodsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeAcademicPeriodDto>>(AcademicPeriods.ToList());

    public Task<IReadOnlyList<FrappeProgramDto>> GetProgramsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeProgramDto>>(Programs.ToList());

    public Task<IReadOnlyList<FrappeCourseDto>> GetCoursesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeCourseDto>>(Courses.ToList());

    public Task<IReadOnlyList<FrappeProgramCourseDto>> GetProgramCoursesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeProgramCourseDto>>(ProgramCourses.ToList());

    public Task<IReadOnlyList<FrappeStudentDto>> GetStudentsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeStudentDto>>(Students.ToList());

    public Task<IReadOnlyList<FrappeInstructorDto>> GetInstructorsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeInstructorDto>>(Instructors.ToList());

    public Task<IReadOnlyList<FrappeCourseSectionDto>> GetCourseSectionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeCourseSectionDto>>(CourseSections.ToList());

    public Task<IReadOnlyList<FrappeCourseScheduleDto>> GetCourseSchedulesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeCourseScheduleDto>>(CourseSchedules.ToList());

    public Task<IReadOnlyList<FrappeProgramEnrollmentDto>> GetProgramEnrollmentsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FrappeProgramEnrollmentDto>>(ProgramEnrollments.ToList());
}
