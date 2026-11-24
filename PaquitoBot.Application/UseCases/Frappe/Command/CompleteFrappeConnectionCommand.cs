using MediatR;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Frappe.Command;

public record CompleteFrappeConnectionCommand(string Code, string State) : IRequest<FrappeConnectionResponse>;

public record FrappeConnectionResponse(Guid UserId, string Email, FrappeSyncSummary Sync);

internal sealed class CompleteFrappeConnectionCommandHandler(
    IUnitOfWork unitOfWork, IFrappeOAuthService oauth, IFrappeStudentSyncService sync)
    : IRequestHandler<CompleteFrappeConnectionCommand, FrappeConnectionResponse>
{
    public async Task<FrappeConnectionResponse> Handle(CompleteFrappeConnectionCommand request, CancellationToken cancellationToken)
    {
        var result = await oauth.CompleteAsync(request.Code, request.State, cancellationToken);
        var user = await unitOfWork.Users.GetByIdAsync(result.UserId, cancellationToken);
        if (user is null || !user.IsActive || !string.Equals(user.Email, result.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("The Frappe account does not match the PaquitoBot user.");

        var summary = await sync.SyncAsync(result.AccessToken, user.UserId, result.Email, cancellationToken);
        return new FrappeConnectionResponse(user.UserId, result.Email, summary);
    }
}
