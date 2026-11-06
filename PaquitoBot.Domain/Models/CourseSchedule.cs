using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class CourseSchedule
{
    public Guid ScheduleId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid SectionId { get; set; }

    public Guid? InstructorId { get; set; }

    public DateOnly ClassDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Location { get; set; }

    public string? DeliveryMode { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Instructor? Instructor { get; set; }

    public virtual CourseSection Section { get; set; } = null!;
}
