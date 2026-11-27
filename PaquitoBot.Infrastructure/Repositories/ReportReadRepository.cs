using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

// All grouping, counting, averaging, ordering and limiting run in PostgreSQL.
// Project the grouped results into simple SQL-translatable shapes first;
// construct application DTOs only after materializing the bounded result set.
public sealed class ReportReadRepository(PaquitoBotDbContext context) : IReportReadRepository
{
    private const int MaxGroups = 5000;

    public async Task<IReadOnlyList<EnrollmentReportRowDto>> GetEnrollmentsAsync(
        Guid? programId, Guid? academicPeriodId, CancellationToken cancellationToken = default)
    {
        var query = context.ProgramEnrollments.AsNoTracking();
        if (programId.HasValue)
            query = query.Where(x => x.ProgramId == programId.Value);
        if (academicPeriodId.HasValue)
            query = query.Where(x => x.AcademicPeriodId == academicPeriodId.Value);

        var aggregates = await query
            .Select(x => new
            {
                x.ProgramId,
                ProgramName = x.Program.Name,
                x.AcademicPeriodId,
                PeriodName = x.AcademicPeriod == null ? "Unassigned" : x.AcademicPeriod.Name,
                x.StudentId
            })
            .GroupBy(x => new { x.ProgramId, x.ProgramName, x.AcademicPeriodId, x.PeriodName })
            .Select(group => new
            {
                group.Key.ProgramId,
                group.Key.ProgramName,
                group.Key.AcademicPeriodId,
                group.Key.PeriodName,
                EnrollmentRecords = group.Count(),
                DistinctStudents = group.Select(x => x.StudentId).Distinct().Count()
            })
            .OrderBy(x => x.ProgramName)
            .ThenBy(x => x.PeriodName)
            .Take(MaxGroups + 1)
            .ToListAsync(cancellationToken);

        EnsureWithinLimit(aggregates.Count);
        return aggregates.Select(x => new EnrollmentReportRowDto(
            x.ProgramId, x.ProgramName, x.AcademicPeriodId, x.PeriodName,
            x.EnrollmentRecords, x.DistinctStudents)).ToList();
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
        if (from.HasValue)
            query = query.Where(x => x.AttendanceDate >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.AttendanceDate <= to.Value);

        var aggregates = await query
            .Select(x => new
            {
                x.SectionId,
                CourseName = x.Section.Course.Name,
                SectionCode = x.Section.SectionCode,
                Status = x.Status.ToLower()
            })
            .GroupBy(x => new { x.SectionId, x.CourseName, x.SectionCode })
            .Select(group => new
            {
                group.Key.SectionId,
                group.Key.CourseName,
                group.Key.SectionCode,
                Records = group.Count(),
                Present = group.Count(x => x.Status == "present"),
                Absent = group.Count(x => x.Status == "absent"),
                Late = group.Count(x => x.Status == "late"),
                Excused = group.Count(x => x.Status == "excused" ||
                                            x.Status == "on leave" || x.Status == "leave"),
                Other = group.Count(x => x.Status != "present" &&
                                          x.Status != "absent" &&
                                          x.Status != "late" &&
                                          x.Status != "excused" &&
                                          x.Status != "on leave" &&
                                          x.Status != "leave")
            })
            .OrderBy(x => x.CourseName)
            .ThenBy(x => x.SectionCode)
            .Take(MaxGroups + 1)
            .ToListAsync(cancellationToken);

        EnsureWithinLimit(aggregates.Count);
        return aggregates.Select(x => new AttendanceReportRowDto(
            x.SectionId, x.CourseName, x.SectionCode ?? "", x.Records,
            x.Present, x.Absent, x.Late, x.Excused, x.Other)).ToList();
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

        var aggregates = await query
            .Select(x => new
            {
                x.Assessment.SectionId,
                CourseName = x.Assessment.Section.Course.Name,
                SectionCode = x.Assessment.Section.SectionCode,
                ScorePercent = 100m * x.Grade!.Value / x.Assessment.MaximumScore!.Value
            })
            .GroupBy(x => new { x.SectionId, x.CourseName, x.SectionCode })
            .Select(group => new
            {
                group.Key.SectionId,
                group.Key.CourseName,
                group.Key.SectionCode,
                GradedResults = group.Count(),
                AverageScorePercent = group.Average(x => x.ScorePercent)
            })
            .OrderBy(x => x.CourseName)
            .ThenBy(x => x.SectionCode)
            .Take(MaxGroups + 1)
            .ToListAsync(cancellationToken);

        EnsureWithinLimit(aggregates.Count);
        return aggregates.Select(x => new AssessmentReportRowDto(
            x.SectionId, x.CourseName, x.SectionCode ?? "",
            x.GradedResults, x.AverageScorePercent)).ToList();
    }

    private static void EnsureWithinLimit(int groupCount)
    {
        if (groupCount > MaxGroups)
            throw new ConflictException("Report exceeds the 5000-group export limit. Narrow the filters.");
    }
}
