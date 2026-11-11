using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.AssessmentPlans;

public record AssessmentPlanDto(
    Guid AssessmentPlanId,
    string? FrappeExternalId,
    Guid SectionId,
    string Title,
    string? Description,
    DateOnly? ScheduledDate,
    decimal? MaximumScore,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static AssessmentPlanDto From(AssessmentPlan plan)
    {
        return new AssessmentPlanDto(
            plan.AssessmentPlanId,
            plan.FrappeExternalId,
            plan.SectionId,
            plan.Title,
            plan.Description,
            plan.ScheduledDate,
            plan.MaximumScore,
            plan.Status,
            plan.FrappeModifiedAt,
            plan.SyncedAt,
            plan.CreatedAt,
            plan.UpdatedAt
        );
    }
}
