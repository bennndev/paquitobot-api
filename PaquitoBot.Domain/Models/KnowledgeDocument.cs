namespace PaquitoBot.Domain.Models;

public partial class KnowledgeDocument
{
    public Guid DocumentId { get; set; }

    public Guid? CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? DocumentType { get; set; }

    public string? Source { get; set; }

    public string? SourceUrl { get; set; }

    public string? Content { get; set; }

    public string? ContentHash { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? IndexedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Course? Course { get; set; }
}
