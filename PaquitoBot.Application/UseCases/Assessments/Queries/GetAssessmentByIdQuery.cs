using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Assessments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Queries;

public record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<AssessmentDto>;

internal sealed class GetAssessmentByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAssessmentByIdQuery, AssessmentDto>
{
    public async Task<AssessmentDto> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _unitOfWork.Repository<Assessment>().GetByIdAsync(request.AssessmentId, cancellationToken)
                         ?? throw new NotFoundException("Assessment", request.AssessmentId);

        return AssessmentDto.From(assessment);
    }
}
