using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<IReadOnlyList<Conversation>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
