using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Chat;

public class SimulatedAnswerProvider : IAnswerProvider
{
    public Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<KnowledgeSource> sources,
        CancellationToken cancellationToken = default)
    {
        if (sources.Count == 0)
        {
            return Task.FromResult("I could not find information about that in the knowledge base.");
        }

        var details = string.Join(" ", sources.Select(source => $"{source.Title}: {source.Snippet}"));

        return Task.FromResult($"Based on {sources.Count} document(s): {details}");
    }
}
