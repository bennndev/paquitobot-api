using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetAssessmentResultByIdUseCaseQuery(Guid ResultId) : IRequest<AssessmentResultDto>;

internal sealed class GetAssessmentResultByIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentResultByIdUseCaseQuery, AssessmentResultDto>
{
    public async Task<AssessmentResultDto> Handle(
        GetAssessmentResultByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Repository<AssessmentResult>().GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException("AssessmentResult", request.ResultId);

        return AssessmentResultDto.From(result);
    }
}
