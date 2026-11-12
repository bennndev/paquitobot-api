using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

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
