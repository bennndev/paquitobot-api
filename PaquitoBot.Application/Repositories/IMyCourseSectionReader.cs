using PaquitoBot.Application.DTOs.Courses;

namespace PaquitoBot.Application.Repositories;

public interface IMyCourseSectionReader
{
    Task<IReadOnlyList<MyCourseSectionDto>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken);
}
