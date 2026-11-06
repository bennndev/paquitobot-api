using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class TuitionFee
{
    public Guid TuitionFeeId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? FeeScheduleId { get; set; }

    public string Description { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public decimal OriginalAmount { get; set; }

    public decimal? OutstandingBalance { get; set; }

    public DateOnly? IssuedOn { get; set; }

    public DateOnly? DueDate { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual FeeSchedule? FeeSchedule { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Student Student { get; set; } = null!;
}
