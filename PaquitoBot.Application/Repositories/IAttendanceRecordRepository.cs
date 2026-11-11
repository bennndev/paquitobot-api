using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IAttendanceRecordRepository : IGenericRepository<AttendanceRecord>
{
    Task<IReadOnlyList<AttendanceRecord>> GetByStudentAndSectionAsync(Guid studentId, Guid sectionId, CancellationToken cancellationToken = default);
}
