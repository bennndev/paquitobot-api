using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentPlans.Command;

public record CreateAssessmentPlanCommand(
    Guid SectionId,
    string Title,
    string? FrappeExternalId = null,
    string? Description = null,
    DateOnly? ScheduledDate = null,
    decimal? MaximumScore = null,
    string? Status = null
) : IRequest<Guid>;

public class CreateAssessmentPlanCommandValidator : AbstractValidator<CreateAssessmentPlanCommand>
{
    public CreateAssessmentPlanCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.SectionId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.ScheduledDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(c => c.ScheduledDate.HasValue);
        RuleFor(c => c.MaximumScore).PrecisionScale(10, 2, ignoreTrailingZeros: true).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class CreateAssessmentPlanCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAssessmentPlanCommand, Guid>
{
    public async Task<Guid> Handle(CreateAssessmentPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = new AssessmentPlan
        {
            SectionId = request.SectionId,
            Title = TextNormalizer.Required(request.Title),
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            Description = TextNormalizer.Optional(request.Description),
            ScheduledDate = request.ScheduledDate,
            MaximumScore = request.MaximumScore,
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<AssessmentPlan>().AddAsync(plan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return plan.AssessmentPlanId;
    }
}
