namespace PaquitoBot.Domain.Models;

public partial class AcademicPeriod
{
    public Guid AcademicPeriodId { get; set; }

    public string? FrappeExternalId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public virtual ICollection<FeeSchedule> FeeSchedules { get; set; } = new List<FeeSchedule>();

    public virtual ICollection<ProgramEnrollment> ProgramEnrollments { get; set; } = new List<ProgramEnrollment>();
}
