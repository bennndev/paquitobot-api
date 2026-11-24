using System.Text.RegularExpressions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Constants;

namespace PaquitoBot.Infrastructure.Services;

public class DocumentKnowledgeRetriever(IUnitOfWork unitOfWork) : IKnowledgeRetriever
{
    private const int MaxTerms = 5;
    private const int MaxSources = 3;
    private const int MinTermLength = 4;
    private const int SnippetLength = 300;

    public async Task<IReadOnlyList<KnowledgeSource>> RetrieveAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        var terms = Regex.Split(question, @"\W+")
            .Where(word => word.Length >= MinTermLength)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxTerms);

        var sources = new Dictionary<Guid, KnowledgeSource>();

        foreach (var term in terms)
        {
            var documents = await unitOfWork.KnowledgeDocuments.SearchAsync(
                null,
                KnowledgeDocumentStatus.Indexed,
                term,
                0,
                MaxSources,
                cancellationToken);

            foreach (var document in documents)
            {
                if (string.IsNullOrWhiteSpace(document.Content))
                {
                    continue;
                }

                var snippet = document.Content.Length <= SnippetLength
                    ? document.Content
                    : document.Content[..SnippetLength];

                sources.TryAdd(document.DocumentId, new KnowledgeSource(document.DocumentId, document.Title, snippet));
            }
        }

        return sources.Values.Take(MaxSources).ToList();
    }
}
