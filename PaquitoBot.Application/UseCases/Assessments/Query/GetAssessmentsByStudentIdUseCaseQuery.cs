using MediatR;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

public record GetAssessmentsByStudentIdUseCaseQuery(Guid StudentId) : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetAssessmentsByStudentIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentsByStudentIdUseCaseQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAssessmentsByStudentIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Assessments.GetByStudentIdAsync(request.StudentId, cancellationToken);
        return items.Select(AssessmentDto.From).ToList();
    }
}
