using PaquitoBot.Application.Dtos.SyncRuns;
using MediatR;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.SyncRuns.Queries;


public record GetLatestSyncRunByEntityQuery : IRequest<IReadOnlyList<GetLatestSyncRunByEntityResponse>>;

internal sealed class GetLatestSyncRunByEntityQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetLatestSyncRunByEntityQuery, IReadOnlyList<GetLatestSyncRunByEntityResponse>>
{
    public async Task<IReadOnlyList<GetLatestSyncRunByEntityResponse>> Handle(
        GetLatestSyncRunByEntityQuery request,
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
