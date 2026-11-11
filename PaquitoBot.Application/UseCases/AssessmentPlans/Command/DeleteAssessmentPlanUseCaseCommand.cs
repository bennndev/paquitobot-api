using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Command;

public record DeleteAssessmentPlanUseCaseCommand(Guid AssessmentPlanId) : IRequest;

internal sealed class DeleteAssessmentPlanUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAssessmentPlanUseCaseCommand>
{
    public async Task Handle(DeleteAssessmentPlanUseCaseCommand request, CancellationToken cancellationToken)
    {
        var plan = await unitOfWork.Repository<AssessmentPlan>().GetByIdAsync(request.AssessmentPlanId, cancellationToken)
            ?? throw new NotFoundException("AssessmentPlan", request.AssessmentPlanId);

        unitOfWork.Repository<AssessmentPlan>().Remove(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
