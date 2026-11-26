using MediatR;
using PaquitoBot.Application.Dtos.Users;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Queries;

public record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;

internal sealed class GetAllUsersQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);

        return users.Select(UserDto.From).ToList();
    }
}
