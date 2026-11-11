using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IAssessmentPlanRepository : IGenericRepository<AssessmentPlan>
{
    Task<IReadOnlyList<AssessmentPlan>> GetBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssessmentPlan>> GetUpcomingAsync(DateOnly fromDate, Guid? sectionId = null, CancellationToken cancellationToken = default);
}
