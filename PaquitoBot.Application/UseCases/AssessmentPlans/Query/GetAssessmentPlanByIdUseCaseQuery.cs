using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

public record GetAssessmentPlanByIdUseCaseQuery(Guid AssessmentPlanId) : IRequest<AssessmentPlanDto>;

internal sealed class GetAssessmentPlanByIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentPlanByIdUseCaseQuery, AssessmentPlanDto>
{
    public async Task<AssessmentPlanDto> Handle(
        GetAssessmentPlanByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var plan = await unitOfWork.Repository<AssessmentPlan>().GetByIdAsync(request.AssessmentPlanId, cancellationToken)
            ?? throw new NotFoundException("AssessmentPlan", request.AssessmentPlanId);

        return AssessmentPlanDto.From(plan);
    }
}
