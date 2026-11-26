using MediatR;
using PaquitoBot.Application.Dtos.Assessments;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.Assessments.Queries;

public record GetAssessmentsBySectionIdQuery(Guid SectionId) : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetAssessmentsBySectionIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentsBySectionIdQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAssessmentsBySectionIdQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Assessments.GetBySectionIdAsync(request.SectionId, cancellationToken);
        return items.Select(AssessmentDto.From).ToList();
    }
}
