using MediatR;
using PaquitoBot.Application.Dtos.AssessmentPlans;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Queries;

public record GetUpcomingAssessmentPlansQuery(Guid? SectionId = null) : IRequest<IReadOnlyList<AssessmentPlanDto>>;

internal sealed class GetUpcomingAssessmentPlansQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUpcomingAssessmentPlansQuery, IReadOnlyList<AssessmentPlanDto>>
{
    public async Task<IReadOnlyList<AssessmentPlanDto>> Handle(GetUpcomingAssessmentPlansQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentPlans.GetUpcomingAsync(DateOnly.FromDateTime(DateTime.UtcNow), request.SectionId, cancellationToken);
        return items.Select(AssessmentPlanDto.From).ToList();
    }
}
