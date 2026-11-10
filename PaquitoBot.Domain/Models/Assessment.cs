namespace PaquitoBot.Domain.Models;

public partial class Assessment
{
    public Guid AssessmentId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid SectionId { get; set; }

    public string Title { get; set; } = null!;

    public string? AssessmentType { get; set; }

    public decimal? MaximumScore { get; set; }

    public decimal? Weight { get; set; }

    public DateOnly? AssessmentDate { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();

    public virtual CourseSection Section { get; set; } = null!;
}
