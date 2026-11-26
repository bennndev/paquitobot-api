using MediatR;
using PaquitoBot.Application.Dtos.AssessmentsResults;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Queries;

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
