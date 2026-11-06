using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class ProgramEnrollment
{
    public Guid ProgramEnrollmentId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid StudentId { get; set; }

    public Guid ProgramId { get; set; }

    public Guid? AcademicPeriodId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AcademicPeriod? AcademicPeriod { get; set; }

    public virtual Program Program { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
