using PaquitoBot.Application.Dtos.StudentFinances;

namespace PaquitoBot.Application.Services;

public interface IFrappeFeesPreviewService
{
   Task<FrappeFinancialPreviewDto> ExecuteAsync(Guid studentId, string sourceDocumentType, CancellationToken cancellationToken);
}
