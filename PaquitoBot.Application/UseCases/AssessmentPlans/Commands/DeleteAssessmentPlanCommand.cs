using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Commands;

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
