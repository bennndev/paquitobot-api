using MediatR;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

public record GetAssessmentsBySectionIdUseCaseQuery(Guid SectionId) : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetAssessmentsBySectionIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentsBySectionIdUseCaseQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAssessmentsBySectionIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Assessments.GetBySectionIdAsync(request.SectionId, cancellationToken);
        return items.Select(AssessmentDto.From).ToList();
    }
}
