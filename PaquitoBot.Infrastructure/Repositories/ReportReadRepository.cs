using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

// Aggregate in PostgreSQL. Never track or modify entities.
// Auto-registered as IReportReadRepository by the existing repository scanner.
public sealed class ReportReadRepository(PaquitoBotDbContext context) : IReportReadRepository
{
    private const int MaxGroups = 5000;

    public async Task<IReadOnlyList<EnrollmentReportRowDto>> GetEnrollmentsAsync(
        Guid? programId, Guid? academicPeriodId, CancellationToken cancellationToken = default)
    {
        var query = context.ProgramEnrollments.AsNoTracking();
        if (programId.HasValue) query = query.Where(x => x.ProgramId == programId.Value);
        if (academicPeriodId.HasValue) query = query.Where(x => x.AcademicPeriodId == academicPeriodId.Value);

        var rows = await query
            .GroupBy(x => new
            {
                x.ProgramId,
                ProgramName = x.Program.Name,
                x.AcademicPeriodId,
                PeriodName = x.AcademicPeriod == null ? "Unassigned" : x.AcademicPeriod.Name
            })
            .Select(group => new EnrollmentReportRowDto(
                group.Key.ProgramId, group.Key.ProgramName, group.Key.AcademicPeriodId,
                group.Key.PeriodName, group.Count(), group.Select(x => x.StudentId).Distinct().Count()))
            .OrderBy(x => x.ProgramName).ThenBy(x => x.AcademicPeriodName)
            .Take(MaxGroups + 1)
            .ToListAsync(cancellationToken);

        EnsureWithinLimit(rows.Count);
        return rows;
    }

    public async Task<IReadOnlyList<AttendanceReportRowDto>> GetAttendanceAsync(
        Guid? programId, Guid? academicPeriodId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.AttendanceRecords.AsNoTracking();
        if (academicPeriodId.HasValue)
            query = query.Where(x => x.Section.AcademicPeriodId == academicPeriodId.Value);
        if (programId.HasValue)
            query = query.Where(x => context.ProgramCourses.Any(link =>
                link.ProgramId == programId.Value && link.CourseId == x.Section.CourseId));
        if (from.HasValue) query = query.Where(x => x.AttendanceDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.AttendanceDate <= to.Value);

        var rows = await query
            .GroupBy(x => new
            {
                x.SectionId,
                CourseName = x.Section.Course.Name,
                SectionCode = x.Section.SectionCode
            })
            .Select(group => new AttendanceReportRowDto(
                group.Key.SectionId,
                group.Key.CourseName,
                group.Key.SectionCode ?? "",
                group.Count(),
                group.Count(x => x.Status.ToLower() == "present"),
                group.Count(x => x.Status.ToLower() == "absent"),
                group.Count(x => x.Status.ToLower() == "late"),
                group.Count(x => x.Status.ToLower() == "excused" ||
                                 x.Status.ToLower() == "on leave" || x.Status.ToLower() == "leave"),
                group.Count(x => x.Status.ToLower() != "present" &&
                                 x.Status.ToLower() != "absent" &&
                                 x.Status.ToLower() != "late" &&
                                 x.Status.ToLower() != "excused" &&
                                 x.Status.ToLower() != "on leave" &&
                                 x.Status.ToLower() != "leave")))
            .OrderBy(x => x.CourseName).ThenBy(x => x.SectionCode)
            .Take(MaxGroups + 1)
            .ToListAsync(cancellationToken);

        EnsureWithinLimit(rows.Count);
        return rows;
    }

    public async Task<IReadOnlyList<AssessmentReportRowDto>> GetAssessmentsAsync(
        Guid? programId, Guid? academicPeriodId, CancellationToken cancellationToken = default)
    {
        var query = context.AssessmentResults.AsNoTracking()
            .Where(x => x.Grade.HasValue && x.Assessment.MaximumScore.HasValue &&
                        x.Assessment.MaximumScore.Value > 0m);
        if (academicPeriodId.HasValue)
            query = query.Where(x => x.Assessment.Section.AcademicPeriodId == academicPeriodId.Value);
        if (programId.HasValue)
            query = query.Where(x => context.ProgramCourses.Any(link =>
                link.ProgramId == programId.Value && link.CourseId == x.Assessment.Section.CourseId));

        var rows = await query
            .GroupBy(x => new
            {
                x.Assessment.SectionId,
                CourseName = x.Assessment.Section.Course.Name,
                SectionCode = x.Assessment.Section.SectionCode
            })
            .Select(group => new AssessmentReportRowDto(
                group.Key.SectionId,
                group.Key.CourseName,
                group.Key.SectionCode ?? "",
                group.Count(),
                group.Average(x => 100m * x.Grade!.Value / x.Assessment.MaximumScore!.Value)))
            .OrderBy(x => x.CourseName).ThenBy(x => x.SectionCode)
            .Take(MaxGroups + 1)
            .ToListAsync(cancellationToken);

        EnsureWithinLimit(rows.Count);
        return rows;
    }

    private static void EnsureWithinLimit(int groupCount)
    {
        if (groupCount > MaxGroups)
            throw new ConflictException("Report exceeds the 5000-group export limit. Narrow the filters.");
    }
}
