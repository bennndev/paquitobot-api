using MediatR;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetResultsByStudentIdQuery(Guid StudentId) : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetResultsByStudentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetResultsByStudentIdQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(GetResultsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentResults.GetByStudentIdAsync(request.StudentId, cancellationToken);
        return items.Select(AssessmentResultDto.From).ToList();
    }
}
