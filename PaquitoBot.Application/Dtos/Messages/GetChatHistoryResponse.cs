namespace PaquitoBot.Application.Dtos.Messages;

public record GetChatHistoryResponse(
    Guid MessageId,
    string SenderType,
    string Content,
    IReadOnlyList<GetChatHistorySourceResponse> Sources,
    DateTime CreatedAt);
