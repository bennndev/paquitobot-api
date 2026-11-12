using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Command;

public record DeleteSyncRunCommand(Guid SyncRunId) : IRequest;

public class DeleteSyncRunCommandValidator : AbstractValidator<DeleteSyncRunCommand>
{
    public DeleteSyncRunCommandValidator()
    {
        RuleFor(c => c.SyncRunId).NotEmpty();
    }
}

public class DeleteSyncRunCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteSyncRunCommand>
{
    public async Task Handle(DeleteSyncRunCommand request, CancellationToken cancellationToken)
    {
        var syncRun = await _unitOfWork.Repository<SyncRun>().GetByIdAsync(request.SyncRunId, cancellationToken)
            ?? throw new NotFoundException("SyncRun", request.SyncRunId);

        _unitOfWork.Repository<SyncRun>().Remove(syncRun);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
