using MediatR;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Query;

public record GetUpcomingAssessmentPlansUseCaseQuery(Guid? SectionId = null) : IRequest<IReadOnlyList<AssessmentPlanDto>>;

internal sealed class GetUpcomingAssessmentPlansUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUpcomingAssessmentPlansUseCaseQuery, IReadOnlyList<AssessmentPlanDto>>
{
    public async Task<IReadOnlyList<AssessmentPlanDto>> Handle(GetUpcomingAssessmentPlansUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentPlans.GetUpcomingAsync(DateOnly.FromDateTime(DateTime.UtcNow), request.SectionId, cancellationToken);
        return items.Select(AssessmentPlanDto.From).ToList();
    }
}
