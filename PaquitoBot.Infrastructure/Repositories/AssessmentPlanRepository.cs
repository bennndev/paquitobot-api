using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class AssessmentPlanRepository : GenericRepository<AssessmentPlan>, IAssessmentPlanRepository
{
    public AssessmentPlanRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AssessmentPlan>> GetBySectionIdAsync(Guid sectionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(p => p.SectionId == sectionId)
            .OrderBy(p => p.ScheduledDate).ThenBy(p => p.AssessmentPlanId).ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<AssessmentPlan>> GetUpcomingAsync(DateOnly fromDate, Guid? sectionId = null, CancellationToken cancellationToken = default)
    {
        var plans = DbSet.AsNoTracking().Where(p => p.ScheduledDate >= fromDate);
        if (sectionId.HasValue)
            plans = plans.Where(p => p.SectionId == sectionId.Value);

        return await plans.OrderBy(p => p.ScheduledDate).ThenBy(p => p.AssessmentPlanId).ToListAsync(cancellationToken);
    }

}
