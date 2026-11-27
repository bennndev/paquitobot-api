using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

internal sealed class MyCourseSectionReader(PaquitoBotDbContext context) : IMyCourseSectionReader
{
    public async Task<IReadOnlyList<MyCourseSectionDto>> GetByStudentIdAsync(
        Guid studentId, CancellationToken cancellationToken)
    {
        return await context.Set<CourseSection>()
            .AsNoTracking()
            .Where(section => section.SectionEnrollments.Any(enrollment => enrollment.StudentId == studentId))
            .OrderBy(section => section.CourseId)
            .ThenBy(section => section.SectionCode)
            .ThenBy(section => section.SectionId)
            .Select(section => new MyCourseSectionDto(section.CourseId, section.SectionId, section.SectionCode))
            .ToListAsync(cancellationToken);
    }
}
