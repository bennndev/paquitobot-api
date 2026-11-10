using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public record DeleteUserUseCaseCommand(Guid UserId) : IRequest;

public class DeleteUserUseCaseCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserUseCaseCommand>
{
    public async Task Handle(DeleteUserUseCaseCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        _unitOfWork.Repository<User>().Remove(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
