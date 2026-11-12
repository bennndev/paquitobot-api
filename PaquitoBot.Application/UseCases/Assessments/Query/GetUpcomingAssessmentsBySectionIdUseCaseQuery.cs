using MediatR;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

public record GetUpcomingAssessmentsBySectionIdUseCaseQuery(Guid SectionId) : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetUpcomingAssessmentsBySectionIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUpcomingAssessmentsBySectionIdUseCaseQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetUpcomingAssessmentsBySectionIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Assessments.GetUpcomingBySectionIdAsync(request.SectionId, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        return items.Select(AssessmentDto.From).ToList();
    }
}
