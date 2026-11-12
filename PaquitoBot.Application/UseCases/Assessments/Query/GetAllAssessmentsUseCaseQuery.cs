using MediatR;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

public record GetAllAssessmentsUseCaseQuery : IRequest<IReadOnlyList<AssessmentDto>>;


internal sealed class GetAllAssessmentsUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllAssessmentsUseCaseQuery, IReadOnlyList<AssessmentDto>>
{
    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAllAssessmentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var assessments = await _unitOfWork.Repository<Assessment>().GetAllAsync(cancellationToken);
        return assessments.Select(AssessmentDto.From).ToList();
    }
}