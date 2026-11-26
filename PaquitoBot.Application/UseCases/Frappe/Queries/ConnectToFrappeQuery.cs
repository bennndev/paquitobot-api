using MediatR;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Frappe.Queries;

public record ConnectToFrappeQuery : IRequest<FrappeAuthorization>;

internal sealed class ConnectToFrappeQueryHandler(IUnitOfWork unitOfWork, IFrappeOAuthService oauth, ICurrentUser currentUser)
    : IRequestHandler<ConnectToFrappeQuery, FrappeAuthorization>
{
    public async Task<FrappeAuthorization> Handle(ConnectToFrappeQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException("The PaquitoBot user is not authenticated.");
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("The PaquitoBot user is not active.");

        return oauth.Begin(user.UserId, user.Email);
    }
}
