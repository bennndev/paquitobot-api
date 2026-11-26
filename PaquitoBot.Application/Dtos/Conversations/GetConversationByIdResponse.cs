namespace PaquitoBot.Application.Dtos.Conversations;

public record GetConversationByIdResponse(
    Guid ConversationId,
    Guid UserId,
    string? Title,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
