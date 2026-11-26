using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class CourseScheduleRepository : GenericRepository<CourseSchedule>, ICourseScheduleRepository
{
    public CourseScheduleRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CourseSchedule>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(schedule => schedule.Section)
            .ThenInclude(section => section.Course)
            .Where(schedule => schedule.Section.SectionEnrollments.Any(enrollment =>
                enrollment.StudentId == studentId))
            .OrderBy(schedule => schedule.ClassDate)
            .ThenBy(schedule => schedule.StartTime)
            .ThenBy(schedule => schedule.ScheduleId)
            .ToListAsync(cancellationToken);
    }
}
