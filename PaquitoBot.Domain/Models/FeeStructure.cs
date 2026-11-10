namespace PaquitoBot.Domain.Models;

public partial class FeeStructure
{
    public Guid FeeStructureId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid? ProgramId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<FeeSchedule> FeeSchedules { get; set; } = new List<FeeSchedule>();

    public virtual Program? Program { get; set; }
}
