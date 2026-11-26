using FluentValidation;
using MediatR;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Commands;

public record StartSyncRunCommand(string EntityName) : IRequest<Guid>;

public class StartSyncRunCommandValidator : AbstractValidator<StartSyncRunCommand>
{
    public StartSyncRunCommandValidator()
    {
        RuleFor(c => c.EntityName).NotEmpty().MaximumLength(140);
    }
}

internal sealed class StartSyncRunCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<StartSyncRunCommand, Guid>
{
    public async Task<Guid> Handle(StartSyncRunCommand request, CancellationToken cancellationToken)
    {
        var syncRun = new SyncRun
        {
            EntityName = TextNormalizer.Required(request.EntityName),
            Status = SyncRunStatus.Running,
            StartDate = DateTime.UtcNow,
            ProcessedRecords = 0
        };

        await _unitOfWork.Repository<SyncRun>().AddAsync(syncRun, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return syncRun.SyncRunId;
    }
}
