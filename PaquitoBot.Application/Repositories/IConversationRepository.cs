using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<IReadOnlyList<Conversation>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
