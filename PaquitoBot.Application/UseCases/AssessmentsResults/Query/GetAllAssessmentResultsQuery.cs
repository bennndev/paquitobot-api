using MediatR;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

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
