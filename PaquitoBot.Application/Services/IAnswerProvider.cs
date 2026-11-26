namespace PaquitoBot.Application.Services;

public interface IAnswerProvider
{
    Task<string> GenerateAnswerAsync(
        string question,
        CancellationToken cancellationToken = default);
}
