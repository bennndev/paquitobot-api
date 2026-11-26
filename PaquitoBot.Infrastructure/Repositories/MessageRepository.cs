using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Message>> GetByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetRecentByConversationAsync(
        Guid conversationId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var messages = await DbSet.AsNoTracking()
            .Where(message => message.ConversationId == conversationId)
            .OrderByDescending(message => message.CreatedAt)
            .ThenByDescending(message => message.MessageId)
            .Take(count)
            .ToListAsync(cancellationToken);

        messages.Reverse();
        return messages;
    }

    protected override IOrderedQueryable<Message> DefaultOrder(IQueryable<Message> query)
    {
        return query.OrderBy(m => m.CreatedAt);
    }
}
