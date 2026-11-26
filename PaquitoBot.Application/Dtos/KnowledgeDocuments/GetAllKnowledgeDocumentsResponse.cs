namespace PaquitoBot.Application.Dtos.KnowledgeDocuments;

public record GetAllKnowledgeDocumentsResponse(
    Guid DocumentId,
    Guid? CourseId,
    string Title,
    string? DocumentType,
    string? Source,
    string? SourceUrl,
    string? ContentHash,
    string Status,
    DateTime? IndexedAt,
    DateTime UpdatedAt);
