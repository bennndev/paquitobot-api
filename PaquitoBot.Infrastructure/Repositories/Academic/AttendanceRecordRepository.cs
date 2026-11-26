using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Academic;

public class AttendanceRecordRepository : GenericRepository<AttendanceRecord>, IAttendanceRecordRepository
{
    public AttendanceRecordRepository (PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetByStudentAndSectionAsync(Guid studentId, Guid sectionId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(a => a.StudentId == studentId && a.SectionId == sectionId)
            .OrderByDescending(a => a.AttendanceDate).ThenBy(a => a.AttendanceId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(record => record.StudentId == studentId)
            .OrderByDescending(record => record.AttendanceDate)
            .ThenBy(record => record.AttendanceId)
            .ToListAsync(cancellationToken);
    }

}
