using MediatR;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Query;

public record GetAllUsersUseCaseQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetAllUsersUseCaseQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllUsersUseCaseQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(
        GetAllUsersUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);

        return users.Select(UserDto.From).ToList();
    }
}
