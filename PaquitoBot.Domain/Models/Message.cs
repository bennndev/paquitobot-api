using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class Message
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public string SenderType { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
