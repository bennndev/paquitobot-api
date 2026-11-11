using MediatR;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

public record GetAllAssessmentPlansUseCaseQuery : IRequest<IReadOnlyList<AssessmentPlanDto>>;

internal sealed class GetAllAssessmentPlansUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAssessmentPlansUseCaseQuery, IReadOnlyList<AssessmentPlanDto>>
{
    public async Task<IReadOnlyList<AssessmentPlanDto>> Handle(
        GetAllAssessmentPlansUseCaseQuery request, CancellationToken cancellationToken)
    {
        var plans = await unitOfWork.Repository<AssessmentPlan>().GetAllAsync(cancellationToken);
        return plans.Select(AssessmentPlanDto.From).ToList();
    }
}
