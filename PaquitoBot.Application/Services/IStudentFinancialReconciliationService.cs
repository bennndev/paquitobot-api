using PaquitoBot.Application.Dtos.StudentFinances;

namespace PaquitoBot.Application.Services;

public interface IStudentFinancialReconciliationService
{
   Task<StudentFinancialReconciliationDto> ExecuteAsync(Guid studentId, CancellationToken cancellationToken);
}
