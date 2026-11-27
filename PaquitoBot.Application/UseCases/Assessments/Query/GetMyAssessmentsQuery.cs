using MediatR;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

public record GetMyAssessmentsQuery : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetMyAssessmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetMyAssessmentsQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetMyAssessmentsQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = currentUser.StudentId
            ?? throw new UnauthorizedAccessException("The current token has no student_id. Sign in again after connecting Frappe.");
        var assessments = await unitOfWork.Assessments.GetByStudentIdAsync(studentId, cancellationToken);
        return assessments.Select(AssessmentDto.From).ToList();
    }
}
