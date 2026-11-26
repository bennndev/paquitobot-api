using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest;

internal sealed class DeleteUserCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        _unitOfWork.Repository<User>().Remove(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
