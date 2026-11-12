using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetStudentResultByAssessmentIdQuery(Guid StudentId, Guid AssessmentId)
    : IRequest<AssessmentResultDto>;

internal sealed class GetStudentResultByAssessmentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentResultByAssessmentIdQuery, AssessmentResultDto>
{
    public async Task<AssessmentResultDto> Handle(
        GetStudentResultByAssessmentIdQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.AssessmentResults.GetLatestByStudentAndAssessmentAsync(
            request.StudentId, request.AssessmentId, cancellationToken)
            ?? throw new NotFoundException("Assessment result for the student and assessment was not found.");

        return AssessmentResultDto.From(result);
    }
}
