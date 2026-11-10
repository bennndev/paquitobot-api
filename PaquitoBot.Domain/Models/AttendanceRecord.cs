namespace PaquitoBot.Domain.Models;

public partial class AttendanceRecord
{
    public Guid AttendanceId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid SectionId { get; set; }

    public Guid StudentId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual CourseSection Section { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
