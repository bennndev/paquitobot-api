using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IAssessmentResultRepository : IGenericRepository<AssessmentResult>
{
    Task<IReadOnlyList<AssessmentResult>> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssessmentResult>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<AssessmentResult?> GetLatestByStudentAndAssessmentAsync(Guid studentId, Guid assessmentId, CancellationToken cancellationToken = default);
}
