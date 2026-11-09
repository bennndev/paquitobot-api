using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.Repositories;

public interface IUserStudentRepository : IGenericRepository<UserStudent>
{
    Task<bool> PairExistsAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default);

    Task<bool> UserHasPrimaryAsync(
        Guid userId,
        Guid? excludingLinkId = null,
        CancellationToken cancellationToken = default);
}
