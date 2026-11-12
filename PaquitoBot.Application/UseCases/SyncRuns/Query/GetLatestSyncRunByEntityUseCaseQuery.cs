using MediatR;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.SyncRuns.Query;

public record GetLatestSyncRunByEntityResponse(
    Guid SyncRunId,
    string EntityName,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    int ProcessedRecords,
    string? ErrorMessage);

public record GetLatestSyncRunByEntityUseCaseQuery : IRequest<IReadOnlyList<GetLatestSyncRunByEntityResponse>>;

public class GetLatestSyncRunByEntityUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetLatestSyncRunByEntityUseCaseQuery, IReadOnlyList<GetLatestSyncRunByEntityResponse>>
{
    public async Task<IReadOnlyList<GetLatestSyncRunByEntityResponse>> Handle(
        GetLatestSyncRunByEntityUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var latestRuns = await _unitOfWork.SyncRuns.GetLatestByEntityAsync(cancellationToken);

        return latestRuns
            .OrderBy(s => s.EntityName)
            .Select(s => new GetLatestSyncRunByEntityResponse(
                s.SyncRunId,
                s.EntityName,
                s.Status,
                s.StartDate,
                s.EndDate,
                s.ProcessedRecords,
                s.ErrorMessage))
            .ToList();
    }
}
