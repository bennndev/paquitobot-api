using MediatR;
using PaquitoBot.Application.Dtos.AssessmentsResults;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Queries;

public record GetAllAssessmentResultsQuery : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetAllAssessmentResultsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAssessmentResultsQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(
        GetAllAssessmentResultsQuery request, CancellationToken cancellationToken)
    {
        var results = await unitOfWork.Repository<AssessmentResult>().GetAllAsync(cancellationToken);
        return results.Select(AssessmentResultDto.From).ToList();
    }
}
