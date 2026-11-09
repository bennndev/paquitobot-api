using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public class CreateUserUseCaseCommand
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserUseCaseCommand(IGenericRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Username = RequestValidator.RequireText(request.Username, "Username", 100),
            Email = RequestValidator.RequireEmail(request.Email, "Email"),
            Role = RequestValidator.RequireText(request.Role, "Role", 40),
            IsActive = request.IsActive
        };

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
