using MediatR;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetAllAssessmentResultsUseCaseQuery : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetAllAssessmentResultsUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAssessmentResultsUseCaseQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(
        GetAllAssessmentResultsUseCaseQuery request, CancellationToken cancellationToken)
    {
        var results = await unitOfWork.Repository<AssessmentResult>().GetAllAsync(cancellationToken);
        return results.Select(AssessmentResultDto.From).ToList();
    }
}
