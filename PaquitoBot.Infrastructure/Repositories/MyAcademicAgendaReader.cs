using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.UseCases.StudentAgenda.Query;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

internal sealed class MyAcademicAgendaReader(PaquitoBotDbContext context) : IMyAcademicAgendaReader
{
    public async Task<MyAcademicAgenda> ReadAsync(
        Guid studentId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var classes = await context.CourseSchedules.AsNoTracking()
            .Where(row => row.FrappeExternalId != null && row.FrappeExternalId != "" && row.SyncedAt.HasValue
                && row.ClassDate >= from && row.ClassDate <= to
                && row.Section.SectionEnrollments.Any(enrollment => enrollment.StudentId == studentId))
            .OrderBy(row => row.ClassDate).ThenBy(row => row.StartTime).ThenBy(row => row.ScheduleId)
            .Select(row => new AgendaClass(row.ScheduleId, row.Section.CourseId, row.SectionId,
                row.Section.Course.Name, row.Section.SectionCode, row.ClassDate, row.StartTime, row.EndTime,
                row.Location, row.Status))
            .ToListAsync(cancellationToken);

        var evaluations = await context.AssessmentPlans.AsNoTracking()
            .Where(row => row.FrappeExternalId != null && row.FrappeExternalId != "" && row.SyncedAt.HasValue
                && row.ScheduledDate >= from && row.ScheduledDate <= to
                && row.Section.SectionEnrollments.Any(enrollment => enrollment.StudentId == studentId))
            .OrderBy(row => row.ScheduledDate).ThenBy(row => row.Title).ThenBy(row => row.AssessmentPlanId)
            .Select(row => new AgendaEvaluation(row.AssessmentPlanId, row.Section.CourseId, row.SectionId,
                row.Section.Course.Name, row.Section.SectionCode, row.Title, row.ScheduledDate!.Value, row.Status))
            .ToListAsync(cancellationToken);

        return new MyAcademicAgenda(classes, evaluations);
    }
}
