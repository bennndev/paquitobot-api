namespace PaquitoBot.Application.Services;

public interface IAnswerProvider
{
    Task<ChatModelResponse> BeginAnswerAsync(
        IReadOnlyList<ChatTurn> turns,
        CancellationToken cancellationToken = default);

    Task<string> CompleteToolAsync(ChatToolRequest toolRequest, string result,
        CancellationToken cancellationToken = default);
}

public record ChatToolRequest(string Name, string CallId, string Continuation);

public record ChatModelResponse(string? Answer, ChatToolRequest? ToolRequest);
