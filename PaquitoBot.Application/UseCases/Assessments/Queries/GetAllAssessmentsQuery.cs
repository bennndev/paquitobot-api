using MediatR;
using PaquitoBot.Application.Dtos.Assessments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Queries;

public record GetAllAssessmentsQuery : IRequest<IReadOnlyList<AssessmentDto>>;


internal sealed class GetAllAssessmentsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllAssessmentsQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAllAssessmentsQuery request,
        CancellationToken cancellationToken)
    {
        var assessments = await _unitOfWork.Repository<Assessment>().GetAllAsync(cancellationToken);
        return assessments.Select(AssessmentDto.From).ToList();
    }
}