using MediatR;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetResultsByAssessmentIdQuery(Guid AssessmentId) : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetResultsByAssessmentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetResultsByAssessmentIdQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(GetResultsByAssessmentIdQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentResults.GetByAssessmentIdAsync(request.AssessmentId, cancellationToken);
        return items.Select(AssessmentResultDto.From).ToList();
    }
}
