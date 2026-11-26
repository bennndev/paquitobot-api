using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.AssessmentsResults;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Queries;

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
