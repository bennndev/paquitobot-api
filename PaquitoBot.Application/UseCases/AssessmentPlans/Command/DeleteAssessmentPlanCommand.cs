using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Command;

public record DeleteAssessmentPlanCommand(Guid AssessmentPlanId) : IRequest;

internal sealed class DeleteAssessmentPlanCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAssessmentPlanCommand>
{
    public async Task Handle(DeleteAssessmentPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await unitOfWork.Repository<AssessmentPlan>().GetByIdAsync(request.AssessmentPlanId, cancellationToken)
            ?? throw new NotFoundException("AssessmentPlan", request.AssessmentPlanId);

        unitOfWork.Repository<AssessmentPlan>().Remove(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
