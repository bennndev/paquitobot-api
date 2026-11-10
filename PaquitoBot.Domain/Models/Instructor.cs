namespace PaquitoBot.Domain.Models;

public partial class Instructor
{
    public Guid InstructorId { get; set; }

    public string? FrappeExternalId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();
}
