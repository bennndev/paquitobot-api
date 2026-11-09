using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public class UpdateUserUseCaseCommand
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserUseCaseCommand(IGenericRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> ExecuteAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = RequestValidator.RequireText(request.Username, "Username", 100);
        var email = RequestValidator.RequireEmail(request.Email, "Email");
        var role = RequestValidator.RequireText(request.Role, "Role", 40);

        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        user.Username = username;
        user.Email = email;
        user.Role = role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
