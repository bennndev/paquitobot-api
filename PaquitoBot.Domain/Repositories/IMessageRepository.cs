using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IReadOnlyList<Message>> GetByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
