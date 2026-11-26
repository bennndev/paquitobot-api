using PaquitoBot.Application.Dtos.Sync;

namespace PaquitoBot.Application.Services;

public interface IAcademicSyncService
{
   Task<AcademicSyncResponse> ExecuteAsync(CancellationToken cancellationToken);
}
