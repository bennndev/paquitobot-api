namespace PaquitoBot.Domain.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid StudentId { get; set; }

    public Guid TuitionFeeId { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateOnly? PaidOn { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Reference { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual TuitionFee TuitionFee { get; set; } = null!;
}
