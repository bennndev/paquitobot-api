using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application;

public interface IFrappeEducationClient
{
    Task<Student?> GetStudentByExternalIdAsync(string studentExternalId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default);
}
