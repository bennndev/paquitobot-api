using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Chat;

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Conversation>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}
