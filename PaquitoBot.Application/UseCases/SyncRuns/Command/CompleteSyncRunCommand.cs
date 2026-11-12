using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Command;

public record CompleteSyncRunCommand(
    Guid SyncRunId,
    bool Succeeded,
    int ProcessedRecords,
    string? ErrorMessage = null) : IRequest;

public class CompleteSyncRunCommandValidator : AbstractValidator<CompleteSyncRunCommand>
{
    public CompleteSyncRunCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.SyncRunId).NotEmpty();
        RuleFor(c => c.ProcessedRecords).GreaterThanOrEqualTo(0);
        RuleFor(c => c.ErrorMessage).NotEmpty().When(c => !c.Succeeded);
    }
}

public class CompleteSyncRunCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CompleteSyncRunCommand>
{
    public async Task Handle(CompleteSyncRunCommand request, CancellationToken cancellationToken)
    {
        var syncRun = await _unitOfWork.Repository<SyncRun>().GetByIdAsync(request.SyncRunId, cancellationToken)
            ?? throw new NotFoundException("SyncRun", request.SyncRunId);

        if (syncRun.Status != SyncRunStatus.Running)
        {
            throw new ConflictException($"Sync run '{request.SyncRunId}' is already finished.");
        }

        syncRun.Status = request.Succeeded ? SyncRunStatus.Completed : SyncRunStatus.Failed;
        syncRun.EndDate = DateTime.UtcNow;
        syncRun.ProcessedRecords = request.ProcessedRecords;
        syncRun.ErrorMessage = request.Succeeded ? null : TextNormalizer.Optional(request.ErrorMessage);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
