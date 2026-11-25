namespace PaquitoBot.Application.Services;

public record KnowledgeSource(Guid DocumentId, string Title, string Snippet);

public interface IKnowledgeRetriever
{
    Task<IReadOnlyList<KnowledgeSource>> RetrieveAsync(
        string question,
        CancellationToken cancellationToken = default);
}
