using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Users;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

internal sealed class GetUserByIdQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetUserByIdQuery, UserDto>
{

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        return UserDto.From(user);
    }
}
