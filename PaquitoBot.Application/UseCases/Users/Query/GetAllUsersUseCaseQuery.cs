using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Query;

public class GetAllUsersUseCaseQuery
{
    private readonly IGenericRepository<User> _users;

    public GetAllUsersUseCaseQuery(IGenericRepository<User> users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        return users.Select(UserDto.From).ToList();
    }
}
