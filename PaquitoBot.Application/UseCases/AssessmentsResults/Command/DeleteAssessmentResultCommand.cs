using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Command;

public record DeleteAssessmentResultCommand(Guid ResultId) : IRequest;

internal sealed class DeleteAssessmentResultCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAssessmentResultCommand>
{
    public async Task Handle(DeleteAssessmentResultCommand request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Repository<AssessmentResult>().GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException("AssessmentResult", request.ResultId);

        unitOfWork.Repository<AssessmentResult>().Remove(result);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
