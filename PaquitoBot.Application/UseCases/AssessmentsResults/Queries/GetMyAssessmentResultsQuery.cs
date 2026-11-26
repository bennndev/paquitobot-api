using MediatR;
using PaquitoBot.Application.Dtos.AssessmentsResults;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Queries;

public record GetMyAssessmentResultsQuery : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetMyAssessmentResultsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetMyAssessmentResultsQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(GetMyAssessmentResultsQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = currentUser.StudentId
            ?? throw new UnauthorizedAccessException("The current token has no student_id. Sign in again after connecting Frappe.");
        var results = await unitOfWork.AssessmentResults.GetByStudentIdAsync(studentId, cancellationToken);
        return results.Select(AssessmentResultDto.From).ToList();
    }
}
