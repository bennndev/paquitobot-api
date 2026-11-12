using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class AttendanceRecordRepository : GenericRepository<AttendanceRecord>, IAttendanceRecordRepository
{
    public AttendanceRecordRepository (PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetByStudentAndSectionAsync(Guid studentId, Guid sectionId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(a => a.StudentId == studentId && a.SectionId == sectionId)
            .OrderByDescending(a => a.AttendanceDate).ThenBy(a => a.AttendanceId).ToListAsync(cancellationToken);

    // TODO GetAttendancePercentageByStudentAndSection:
    // Replace STATUS_1 and STATUS_2 with the real attendance statuses, decide whether
    // a late arrival counts as present, and define whether the denominator is recorded
    // attendance or scheduled classes before returning a percentage.
}
