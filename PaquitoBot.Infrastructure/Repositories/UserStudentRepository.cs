using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Infrastructure.Repositories;

public class UserStudentRepository : GenericRepository<UserStudent>, IUserStudentRepository
{
    public UserStudentRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public Task<bool> PairExistsAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            l => l.UserId == userId && l.StudentId == studentId,
            cancellationToken);
    }

    public Task<bool> UserHasPrimaryAsync(
        Guid userId,
        Guid? excludingLinkId = null,
        CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            l => l.UserId == userId
                && l.IsPrimary
                && (excludingLinkId == null || l.LinkId != excludingLinkId),
            cancellationToken);
    }

    protected override IOrderedQueryable<UserStudent> DefaultOrder(IQueryable<UserStudent> query)
    {
        return query.OrderBy(l => l.CreatedAt);
    }
}
