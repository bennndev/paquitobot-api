using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Command;

public record UpdateAssessmentPlanCommand(
    Guid AssessmentPlanId,
    Guid SectionId,
    string Title,
    string? FrappeExternalId = null,
    string? Description = null,
    DateOnly? ScheduledDate = null,
    decimal? MaximumScore = null,
    string? Status = null
) : IRequest<Guid>;

public class UpdateAssessmentPlanCommandValidator : AbstractValidator<UpdateAssessmentPlanCommand>
{
    public UpdateAssessmentPlanCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.AssessmentPlanId).NotEmpty();
        RuleFor(c => c.SectionId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.ScheduledDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(c => c.ScheduledDate.HasValue);
        RuleFor(c => c.MaximumScore).PrecisionScale(10, 2, ignoreTrailingZeros: true).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class UpdateAssessmentPlanCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAssessmentPlanCommand, Guid>
{
    public async Task<Guid> Handle(UpdateAssessmentPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await unitOfWork.Repository<AssessmentPlan>().GetByIdAsync(request.AssessmentPlanId, cancellationToken)
            ?? throw new NotFoundException("AssessmentPlan", request.AssessmentPlanId);

        plan.SectionId = request.SectionId;
        plan.Title = TextNormalizer.Required(request.Title);
        plan.FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId);
        plan.Description = TextNormalizer.Optional(request.Description);
        plan.ScheduledDate = request.ScheduledDate;
        plan.MaximumScore = request.MaximumScore;
        plan.Status = TextNormalizer.Optional(request.Status);
        plan.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return plan.AssessmentPlanId;
    }
}
