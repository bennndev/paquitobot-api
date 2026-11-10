namespace PaquitoBot.Domain.Models;

public partial class CourseSection
{
    public Guid SectionId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid CourseId { get; set; }

    public Guid AcademicPeriodId { get; set; }

    public string? SectionCode { get; set; }

    public string? InstructorName { get; set; }

    public string? DeliveryMode { get; set; }

    public int? Capacity { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AcademicPeriod AcademicPeriod { get; set; } = null!;

    public virtual ICollection<AssessmentPlan> AssessmentPlans { get; set; } = new List<AssessmentPlan>();

    public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();

    public virtual ICollection<SectionEnrollment> SectionEnrollments { get; set; } = new List<SectionEnrollment>();
}
