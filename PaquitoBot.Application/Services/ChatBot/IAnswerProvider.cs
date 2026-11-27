using PaquitoBot.Application.DTOs.Chat;

namespace PaquitoBot.Application.Services.ChatBot;

public interface IAnswerProvider
{
    Task<ChatModelResponse> BeginAnswerAsync(
        IReadOnlyList<ChatTurn> turns,
        CancellationToken cancellationToken = default);

    Task<string> CompleteToolAsync(ChatToolRequest toolRequest, string result,
        CancellationToken cancellationToken = default);
}
