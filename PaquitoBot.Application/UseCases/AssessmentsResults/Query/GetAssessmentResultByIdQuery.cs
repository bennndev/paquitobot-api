using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetAssessmentResultByIdQuery(Guid ResultId) : IRequest<AssessmentResultDto>;

internal sealed class GetAssessmentResultByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentResultByIdQuery, AssessmentResultDto>
{
    public async Task<AssessmentResultDto> Handle(
        GetAssessmentResultByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Repository<AssessmentResult>().GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException("AssessmentResult", request.ResultId);

        return AssessmentResultDto.From(result);
    }
}
