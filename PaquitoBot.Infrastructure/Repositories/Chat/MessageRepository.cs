using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Chat;

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

    protected override IOrderedQueryable<Message> DefaultOrder(IQueryable<Message> query)
    {
        return query.OrderBy(m => m.CreatedAt);
    }
}
