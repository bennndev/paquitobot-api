using MediatR;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Frappe.Commands;

public record CompleteFrappeConnectionCommand(string Code, string State) : IRequest<FrappeConnectionResponse>;

internal sealed class CompleteFrappeConnectionCommandHandler(
    IUnitOfWork unitOfWork, IFrappeOAuthService oauth, IFrappeSnapshotPersister persister, ITokenService tokenService)
    : IRequestHandler<CompleteFrappeConnectionCommand, FrappeConnectionResponse>
{
    public async Task<FrappeConnectionResponse> Handle(CompleteFrappeConnectionCommand request, CancellationToken cancellationToken)
    {
        var result = await oauth.CompleteAsync(request.Code, request.State, cancellationToken);
        var user = await unitOfWork.Users.GetByIdAsync(result.UserId, cancellationToken);
        if (user is null || !user.IsActive ||
            !string.Equals(user.Email, result.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("The Frappe account does not match the PaquitoBot user.");

        var summary = await persister.PersistAsync(result.AccessToken, user.UserId, result.Email, cancellationToken);
        var studentId = await unitOfWork.UserStudents.GetPrimaryStudentIdAsync(user.UserId, cancellationToken);
        var token = tokenService.GenerateToken(user.UserId, user.Role, user.Email, studentId);
        return new FrappeConnectionResponse(user.UserId, result.Email, summary, token);
    }
}
