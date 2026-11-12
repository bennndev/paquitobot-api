using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Command;

public record StartSyncRunCommand(string EntityName) : IRequest<Guid>;

public class StartSyncRunCommandValidator : AbstractValidator<StartSyncRunCommand>
{
    public StartSyncRunCommandValidator()
    {
        RuleFor(c => c.EntityName).NotEmpty().MaximumLength(140);
    }
}

public class StartSyncRunCommandHandler(IUnitOfWork _unitOfWork)
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
