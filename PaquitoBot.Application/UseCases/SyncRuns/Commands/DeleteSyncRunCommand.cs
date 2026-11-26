using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Commands;

public record DeleteSyncRunCommand(Guid SyncRunId) : IRequest;

public class DeleteSyncRunCommandValidator : AbstractValidator<DeleteSyncRunCommand>
{
    public DeleteSyncRunCommandValidator()
    {
        RuleFor(c => c.SyncRunId).NotEmpty();
    }
}

internal sealed class DeleteSyncRunCommandHandler(IUnitOfWork _unitOfWork)
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
