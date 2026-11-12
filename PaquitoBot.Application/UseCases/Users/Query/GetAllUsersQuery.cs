using MediatR;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Query;

public record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetAllUsersQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);

        return users.Select(UserDto.From).ToList();
    }
}
