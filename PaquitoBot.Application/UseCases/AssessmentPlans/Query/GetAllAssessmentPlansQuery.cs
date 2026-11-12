using MediatR;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

public record GetAllAssessmentPlansQuery : IRequest<IReadOnlyList<AssessmentPlanDto>>;

internal sealed class GetAllAssessmentPlansQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAssessmentPlansQuery, IReadOnlyList<AssessmentPlanDto>>
{
    public async Task<IReadOnlyList<AssessmentPlanDto>> Handle(
        GetAllAssessmentPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await unitOfWork.Repository<AssessmentPlan>().GetAllAsync(cancellationToken);
        return plans.Select(AssessmentPlanDto.From).ToList();
    }
}
