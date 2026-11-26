using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.AssessmentPlans;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Queries;

public record GetAssessmentPlanByIdQuery(Guid AssessmentPlanId) : IRequest<AssessmentPlanDto>;

internal sealed class GetAssessmentPlanByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentPlanByIdQuery, AssessmentPlanDto>
{
    public async Task<AssessmentPlanDto> Handle(
        GetAssessmentPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await unitOfWork.Repository<AssessmentPlan>().GetByIdAsync(request.AssessmentPlanId, cancellationToken)
            ?? throw new NotFoundException("AssessmentPlan", request.AssessmentPlanId);

        return AssessmentPlanDto.From(plan);
    }
}
