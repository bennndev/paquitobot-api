using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Constants;

namespace PaquitoBot.Application.UseCases.SyncRuns.Query;

public record GetAllSyncRunsResponse(
    Guid SyncRunId,
    string EntityName,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    int ProcessedRecords,
    string? ErrorMessage);

public record GetAllSyncRunsQuery(
    string? EntityName = null,
    string? Status = null) : IRequest<IReadOnlyList<GetAllSyncRunsResponse>>;

public class GetAllSyncRunsQueryValidator : AbstractValidator<GetAllSyncRunsQuery>
{
    private static readonly string[] KnownStatuses =
    [
        SyncRunStatus.Running,
        SyncRunStatus.Completed,
        SyncRunStatus.Failed
    ];

    public GetAllSyncRunsQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(q => q.EntityName).MaximumLength(140);
        RuleFor(q => q.Status)
            .Must(status => KnownStatuses.Contains(status!.Trim().ToLowerInvariant()))
            .When(q => !string.IsNullOrWhiteSpace(q.Status))
            .WithMessage($"Status must be one of: {string.Join(", ", KnownStatuses)}.");
    }
}

public class GetAllSyncRunsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllSyncRunsQuery, IReadOnlyList<GetAllSyncRunsResponse>>
{
    public async Task<IReadOnlyList<GetAllSyncRunsResponse>> Handle(
        GetAllSyncRunsQuery request,
        CancellationToken cancellationToken)
    {
        var entityName = TextNormalizer.Optional(request.EntityName);
        var status = TextNormalizer.Optional(request.Status)?.ToLowerInvariant();

        var syncRuns = await _unitOfWork.SyncRuns.GetFilteredAsync(entityName, status, cancellationToken);

        return syncRuns
            .Select(s => new GetAllSyncRunsResponse(
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
