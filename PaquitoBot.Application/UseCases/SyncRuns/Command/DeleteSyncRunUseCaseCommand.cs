using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.SyncRuns.Command;

public record DeleteSyncRunUseCaseCommand(Guid SyncRunId) : IRequest;

public class DeleteSyncRunUseCaseCommandValidator : AbstractValidator<DeleteSyncRunUseCaseCommand>
{
    public DeleteSyncRunUseCaseCommandValidator()
    {
        RuleFor(c => c.SyncRunId).NotEmpty();
    }
}

public class DeleteSyncRunUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteSyncRunUseCaseCommand>
{
    public async Task Handle(DeleteSyncRunUseCaseCommand request, CancellationToken cancellationToken)
    {
        var syncRun = await _unitOfWork.Repository<SyncRun>().GetByIdAsync(request.SyncRunId, cancellationToken)
            ?? throw new NotFoundException("SyncRun", request.SyncRunId);

        _unitOfWork.Repository<SyncRun>().Remove(syncRun);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
