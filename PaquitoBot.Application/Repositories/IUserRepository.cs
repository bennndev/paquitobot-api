using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
