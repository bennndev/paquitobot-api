namespace PaquitoBot.Domain.Models;

public partial class SyncRun
{
    public Guid SyncRunId { get; set; }

    public string EntityName { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public int ProcessedRecords { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
