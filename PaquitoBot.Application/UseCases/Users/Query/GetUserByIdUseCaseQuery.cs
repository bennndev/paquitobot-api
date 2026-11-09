using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Query;

public class GetUserByIdUseCaseQuery
{
    private readonly IGenericRepository<User> _users;

    public GetUserByIdUseCaseQuery(IGenericRepository<User> users)
    {
        _users = users;
    }

    public async Task<UserDto> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        return UserDto.From(user);
    }
}
