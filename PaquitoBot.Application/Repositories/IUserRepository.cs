using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> TryResetPasswordAsync(Guid userId, string email, string currentHash, string newHash,
        CancellationToken cancellationToken);
}
