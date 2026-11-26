namespace PaquitoBot.Application.Dtos.Chat;

public record AskPaquitoResponse(
    Guid ConversationId,
    Guid QuestionMessageId,
    Guid AnswerMessageId,
    string Answer,
    bool Succeeded,
    IReadOnlyList<AskPaquitoSourceResponse> Sources);
