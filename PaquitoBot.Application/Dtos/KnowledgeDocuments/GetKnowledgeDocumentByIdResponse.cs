namespace PaquitoBot.Application.Dtos.KnowledgeDocuments;

public record GetKnowledgeDocumentByIdResponse(
    Guid DocumentId,
    Guid? CourseId,
    string Title,
    string? DocumentType,
    string? Source,
    string? SourceUrl,
    string? Content,
    string? ContentHash,
    string Status,
    DateTime? IndexedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
