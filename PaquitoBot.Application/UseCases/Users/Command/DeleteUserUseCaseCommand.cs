using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public class DeleteUserUseCaseCommand
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserUseCaseCommand(IGenericRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        _users.Remove(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
