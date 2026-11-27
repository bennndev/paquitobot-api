using MediatR;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services.SyncByStudent;

namespace PaquitoBot.Application.UseCases.SyncByStudent.Query;

public record ConnectToFrappeQuery(Guid UserId) : IRequest<FrappeAuthorization>;

internal sealed class ConnectToFrappeQueryHandler(IUnitOfWork unitOfWork, IFrappeOAuthService oauth)
    : IRequestHandler<ConnectToFrappeQuery, FrappeAuthorization>
{
    public async Task<FrappeAuthorization> Handle(ConnectToFrappeQuery request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("The PaquitoBot user is not active.");

        return oauth.Begin(user.UserId, user.Email);
    }
}
