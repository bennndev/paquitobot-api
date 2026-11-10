namespace PaquitoBot.Domain.Models;

public partial class Student
{
    public Guid StudentId { get; set; }

    public string FrappeExternalId { get; set; } = null!;

    public string? StudentCode { get; set; }

    public string? FirstNames { get; set; }

    public string? LastNames { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Gender { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProgramEnrollment> ProgramEnrollments { get; set; } = new List<ProgramEnrollment>();

    public virtual ICollection<SectionEnrollment> SectionEnrollments { get; set; } = new List<SectionEnrollment>();

    public virtual ICollection<TuitionFee> TuitionFees { get; set; } = new List<TuitionFee>();

    public virtual ICollection<UserStudent> UserStudents { get; set; } = new List<UserStudent>();
}
