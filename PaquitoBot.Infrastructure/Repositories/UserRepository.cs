using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> TryResetPasswordAsync(Guid userId, string email, string currentHash, string newHash,
        CancellationToken cancellationToken) =>
        await DbSet.Where(user => user.UserId == userId && user.IsActive && user.Email == email &&
                user.PasswordHash == currentHash)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(user => user.PasswordHash, newHash)
                .SetProperty(user => user.UpdatedAt, DateTime.UtcNow), cancellationToken) == 1;
}
