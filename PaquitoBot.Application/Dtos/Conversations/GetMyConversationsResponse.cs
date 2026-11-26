namespace PaquitoBot.Application.Dtos.Conversations;

public record GetMyConversationsResponse(
    Guid ConversationId,
    string? Title,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
