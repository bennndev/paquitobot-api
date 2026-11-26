namespace PaquitoBot.Application.Dtos.Messages;

public record GetMessagesByConversationResponse(
    Guid MessageId,
    string SenderType,
    string Content,
    DateTime CreatedAt);
