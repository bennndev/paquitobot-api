using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Command;

public record DeleteAssessmentCommand(Guid AssessmentId) : IRequest;

internal sealed class DeleteAssessmentCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteAssessmentCommand>
{
    public async Task Handle(DeleteAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _unitOfWork.Repository<Assessment>().GetByIdAsync(request.AssessmentId, cancellationToken)
            ?? throw new NotFoundException("Assessment", request.AssessmentId);
        _unitOfWork.Repository<Assessment>().Remove(assessment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
