using PaquitoBot.Application.Dtos.SyncRuns;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Queries;


public record GetSyncRunByIdQuery(Guid SyncRunId) : IRequest<GetSyncRunByIdResponse>;

public class GetSyncRunByIdQueryValidator : AbstractValidator<GetSyncRunByIdQuery>
{
    public GetSyncRunByIdQueryValidator()
    {
        RuleFor(q => q.SyncRunId).NotEmpty();
    }
}

internal sealed class GetSyncRunByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetSyncRunByIdQuery, GetSyncRunByIdResponse>
{
    public async Task<GetSyncRunByIdResponse> Handle(
        GetSyncRunByIdQuery request,
        CancellationToken cancellationToken)
    {
        var syncRun = await _unitOfWork.Repository<SyncRun>().GetByIdAsync(request.SyncRunId, cancellationToken)
            ?? throw new NotFoundException("SyncRun", request.SyncRunId);

        return new GetSyncRunByIdResponse(
            syncRun.SyncRunId,
            syncRun.EntityName,
            syncRun.Status,
            syncRun.StartDate,
            syncRun.EndDate,
            syncRun.ProcessedRecords,
            syncRun.ErrorMessage,
            syncRun.CreatedAt);
    }
}
