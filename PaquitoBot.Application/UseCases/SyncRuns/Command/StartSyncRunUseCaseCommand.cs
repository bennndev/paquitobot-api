using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Command;

public record StartSyncRunUseCaseCommand(string EntityName) : IRequest<Guid>;

public class StartSyncRunUseCaseCommandValidator : AbstractValidator<StartSyncRunUseCaseCommand>
{
    public StartSyncRunUseCaseCommandValidator()
    {
        RuleFor(c => c.EntityName).NotEmpty().MaximumLength(140);
    }
}

public class StartSyncRunUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<StartSyncRunUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(StartSyncRunUseCaseCommand request, CancellationToken cancellationToken)
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
