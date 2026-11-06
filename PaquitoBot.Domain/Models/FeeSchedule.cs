using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class FeeSchedule
{
    public Guid FeeScheduleId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid FeeStructureId { get; set; }

    public Guid? AcademicPeriodId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? DueDate { get; set; }

    public decimal? ScheduledAmount { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AcademicPeriod? AcademicPeriod { get; set; }

    public virtual FeeStructure FeeStructure { get; set; } = null!;

    public virtual ICollection<TuitionFee> TuitionFees { get; set; } = new List<TuitionFee>();
}
