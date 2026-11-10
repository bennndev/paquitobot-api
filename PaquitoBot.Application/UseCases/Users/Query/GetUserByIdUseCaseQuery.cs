using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Query;

public record GetUserByIdUseCaseQuery(Guid UserId) : IRequest<UserDto>;

public class GetUserByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetUserByIdUseCaseQuery, UserDto>
{

    public async Task<UserDto> Handle(GetUserByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        return UserDto.From(user);
    }
}
