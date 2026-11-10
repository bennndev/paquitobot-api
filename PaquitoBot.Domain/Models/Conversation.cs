namespace PaquitoBot.Domain.Models;

public partial class Conversation
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }

    public string? Title { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}
