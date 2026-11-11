using MediatR;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetResultsByAssessmentIdUseCaseQuery(Guid AssessmentId) : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetResultsByAssessmentIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetResultsByAssessmentIdUseCaseQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(GetResultsByAssessmentIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentResults.GetByAssessmentIdAsync(request.AssessmentId, cancellationToken);
        return items.Select(AssessmentResultDto.From).ToList();
    }
}
