using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public record DeleteUserUseCaseCommand(Guid UserId) : IRequest;

public class DeleteUserUseCaseCommandHandler : IRequestHandler<DeleteUserUseCaseCommand>
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserUseCaseCommandHandler(IGenericRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUserUseCaseCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        _users.Remove(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
