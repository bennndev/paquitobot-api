namespace PaquitoBot.Domain.Models;

public partial class AssessmentPlan
{
    public Guid AssessmentPlanId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid SectionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? ScheduledDate { get; set; }

    public decimal? MaximumScore { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual CourseSection Section { get; set; } = null!;
}
