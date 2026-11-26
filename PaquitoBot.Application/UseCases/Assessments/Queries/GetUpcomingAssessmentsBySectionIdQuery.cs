using MediatR;
using PaquitoBot.Application.Dtos.Assessments;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.Assessments.Queries;

public record GetUpcomingAssessmentsBySectionIdQuery(Guid SectionId) : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetUpcomingAssessmentsBySectionIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUpcomingAssessmentsBySectionIdQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetUpcomingAssessmentsBySectionIdQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Assessments.GetUpcomingBySectionIdAsync(request.SectionId, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        return items.Select(AssessmentDto.From).ToList();
    }
}
