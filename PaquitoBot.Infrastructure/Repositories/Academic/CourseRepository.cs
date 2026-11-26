using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Academic;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Course>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(course => course.CourseSections.Any(section =>
                section.SectionEnrollments.Any(enrollment => enrollment.StudentId == studentId)))
            .OrderBy(course => course.Name)
            .ThenBy(course => course.CourseId)
            .ToListAsync(cancellationToken);
    }
}
