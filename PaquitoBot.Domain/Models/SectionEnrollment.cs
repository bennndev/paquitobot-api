namespace PaquitoBot.Domain.Models;

public partial class SectionEnrollment
{
    public Guid SectionEnrollmentId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid StudentId { get; set; }

    public Guid SectionId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }

    public string? Status { get; set; }

    public decimal? FinalGrade { get; set; }

    public decimal? AttendancePercentage { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual CourseSection Section { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
