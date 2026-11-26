namespace PaquitoBot.Application.Services;

public interface IAnswerProvider
{
    Task<string> GenerateAnswerAsync(
        IReadOnlyList<ChatTurn> turns,
        CancellationToken cancellationToken = default);
}
