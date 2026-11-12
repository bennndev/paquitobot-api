using MediatR;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

public record GetAssessmentPlansBySectionIdQuery(Guid SectionId) : IRequest<IReadOnlyList<AssessmentPlanDto>>;

internal sealed class GetAssessmentPlansBySectionIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentPlansBySectionIdQuery, IReadOnlyList<AssessmentPlanDto>>
{
    public async Task<IReadOnlyList<AssessmentPlanDto>> Handle(GetAssessmentPlansBySectionIdQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentPlans.GetBySectionIdAsync(request.SectionId, cancellationToken);
        return items.Select(AssessmentPlanDto.From).ToList();
    }
}
