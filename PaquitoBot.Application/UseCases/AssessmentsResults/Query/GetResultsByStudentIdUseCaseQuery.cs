using MediatR;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Query;

public record GetResultsByStudentIdUseCaseQuery(Guid StudentId) : IRequest<IReadOnlyList<AssessmentResultDto>>;

internal sealed class GetResultsByStudentIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetResultsByStudentIdUseCaseQuery, IReadOnlyList<AssessmentResultDto>>
{
    public async Task<IReadOnlyList<AssessmentResultDto>> Handle(GetResultsByStudentIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AssessmentResults.GetByStudentIdAsync(request.StudentId, cancellationToken);
        return items.Select(AssessmentResultDto.From).ToList();
    }
}
