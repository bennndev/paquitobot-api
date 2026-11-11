using MediatR;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

public record GetAssessmentPlansBySectionIdUseCaseQuery(Guid SectionId) : IRequest<IReadOnlyList<AssessmentPlanDto>>;

internal sealed class GetAssessmentPlansBySectionIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentPlansBySectionIdUseCaseQuery, IReadOnlyList<AssessmentPlanDto>>
{
    public async Task<IReadOnlyList<AssessmentPlanDto>> Handle(GetAssessmentPlansBySectionIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentPlans.GetBySectionIdAsync(request.SectionId, cancellationToken);
        return items.Select(AssessmentPlanDto.From).ToList();
    }
}
