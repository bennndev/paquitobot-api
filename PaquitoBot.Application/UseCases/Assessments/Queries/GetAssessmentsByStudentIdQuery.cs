using MediatR;
using PaquitoBot.Application.Dtos.Assessments;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.Assessments.Queries;

public record GetAssessmentsByStudentIdQuery(Guid StudentId) : IRequest<IReadOnlyList<AssessmentDto>>;

internal sealed class GetAssessmentsByStudentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssessmentsByStudentIdQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAssessmentsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Assessments.GetByStudentIdAsync(request.StudentId, cancellationToken);
        return items.Select(AssessmentDto.From).ToList();
    }
}
