namespace PaquitoBot.Application.Dtos.Messages;

public record GetMessageByIdResponse(
    Guid MessageId,
    Guid ConversationId,
    string SenderType,
    string Content,
    DateTime CreatedAt);
