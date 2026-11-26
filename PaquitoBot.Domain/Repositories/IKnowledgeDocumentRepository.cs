using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IKnowledgeDocumentRepository : IGenericRepository<KnowledgeDocument>
{
    Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(
        Guid? courseId,
        string? status,
        string? titleText,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
