using MediatR;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Query;

public record GetAllUsersUseCaseQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetAllUsersUseCaseQueryHandler : IRequestHandler<GetAllUsersUseCaseQuery, IReadOnlyList<UserDto>>
{
    private readonly IGenericRepository<User> _users;

    public GetAllUsersUseCaseQueryHandler(IGenericRepository<User> users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(
        GetAllUsersUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        return users.Select(UserDto.From).ToList();
    }
}
