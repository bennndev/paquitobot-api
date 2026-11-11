using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IAssessmentRepository : IGenericRepository<Assessment>
{
    Task<IReadOnlyList<Assessment>> GetBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assessment>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assessment>> GetUpcomingBySectionIdAsync(Guid sectionId, DateOnly fromDate, CancellationToken cancellationToken = default);
}
