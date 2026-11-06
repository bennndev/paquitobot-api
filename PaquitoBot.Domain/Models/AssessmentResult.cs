using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class AssessmentResult
{
    public Guid ResultId { get; set; }

    public string? FrappeExternalId { get; set; }

    public Guid AssessmentId { get; set; }

    public Guid StudentId { get; set; }

    public decimal? Grade { get; set; }

    public string? Notes { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Assessment Assessment { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
