using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IAttendanceRecordRepository : IGenericRepository<AttendanceRecord>
{
    Task<IReadOnlyList<AttendanceRecord>> GetByStudentAndSectionAsync(Guid studentId, Guid sectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceRecord>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}
