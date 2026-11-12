using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class AssessmentRepository : GenericRepository<Assessment>, IAssessmentRepository
{
    public AssessmentRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Assessment>> GetBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(a => a.SectionId == sectionId)
            .OrderBy(a => a.AssessmentDate).ThenBy(a => a.AssessmentId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Assessment>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Where(a => Context.SectionEnrollments.Any(e => e.StudentId == studentId && e.SectionId == a.SectionId))
            .OrderBy(a => a.AssessmentDate).ThenBy(a => a.AssessmentId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Assessment>> GetUpcomingBySectionIdAsync(Guid sectionId, DateOnly fromDate, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(a => a.SectionId == sectionId && a.AssessmentDate >= fromDate)
            .OrderBy(a => a.AssessmentDate).ThenBy(a => a.AssessmentId).ToListAsync(cancellationToken);
}
