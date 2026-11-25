namespace PaquitoBot.Application.Services;

public interface IAnswerProvider
{
    Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<KnowledgeSource> sources,
        CancellationToken cancellationToken = default);
}
