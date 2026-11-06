using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class UserStudent
{
    public Guid LinkId { get; set; }

    public Guid UserId { get; set; }

    public Guid StudentId { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
