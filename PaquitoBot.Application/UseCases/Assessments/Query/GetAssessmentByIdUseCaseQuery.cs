using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Query;

public record GetAssessmentByIdUseCaseQuery(Guid AssessmentId) : IRequest<AssessmentDto>;

internal sealed class GetAssessmentByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAssessmentByIdUseCaseQuery, AssessmentDto>
{
    public async Task<AssessmentDto> Handle(GetAssessmentByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _unitOfWork.Repository<Assessment>().GetByIdAsync(request.AssessmentId, cancellationToken)
                         ?? throw new NotFoundException("Assessment", request.AssessmentId);

        return AssessmentDto.From(assessment);
    }
}
