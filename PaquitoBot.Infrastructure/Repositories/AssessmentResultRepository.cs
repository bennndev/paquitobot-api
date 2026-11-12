using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class AssessmentResultRepository : GenericRepository<AssessmentResult>, IAssessmentResultRepository
{
    public AssessmentResultRepository (PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AssessmentResult>> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(r => r.AssessmentId == assessmentId)
            .OrderBy(r => r.StudentId).ThenBy(r => r.ResultId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AssessmentResult>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt).ThenBy(r => r.ResultId).ToListAsync(cancellationToken);

    public Task<AssessmentResult?> GetLatestByStudentAndAssessmentAsync(Guid studentId, Guid assessmentId, CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking().Where(r => r.StudentId == studentId && r.AssessmentId == assessmentId)
            .OrderByDescending(r => r.UpdatedAt).ThenByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.ResultId).FirstOrDefaultAsync(cancellationToken);
}
