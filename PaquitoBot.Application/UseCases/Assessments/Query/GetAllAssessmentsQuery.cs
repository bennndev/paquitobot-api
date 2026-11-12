using MediatR;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

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