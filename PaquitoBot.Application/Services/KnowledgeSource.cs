namespace PaquitoBot.Application.Services;

// Retained to read sources saved in older chat messages.
public record KnowledgeSource(Guid DocumentId, string Title, string Snippet);
