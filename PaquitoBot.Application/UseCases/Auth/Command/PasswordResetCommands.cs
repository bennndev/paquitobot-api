using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Application.UseCases.Auth.Command;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<bool>;
public sealed record ResetPasswordCommand(string Token, string Password) : IRequest<bool>;

public sealed class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetValidator() =>
        RuleFor(request => request.Email).NotEmpty().MaximumLength(254).EmailAddress();
}

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(request => request.Token).NotEmpty().MaximumLength(4096);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(8).MaximumLength(255);
    }
}

internal sealed class RequestPasswordResetHandler(IPasswordResetQueue queue, IPasswordResetDelivery delivery)
    : IRequestHandler<RequestPasswordResetCommand, bool>
{
    public Task<bool> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(delivery.IsConfigured && queue.TryEnqueue(TextNormalizer.RequiredEmail(request.Email)));
}

internal sealed class ResetPasswordHandler(
    IUserRepository users, IPasswordResetDelivery delivery, IUserPasswordHasher hasher)
    : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var ticket = delivery.Unprotect(request.Token);
        if (ticket is null) return false;

        var user = await users.GetByEmailAsync(ticket.Email, cancellationToken);
        if (user is null || user.UserId != ticket.UserId || !user.IsActive || user.PasswordHash != ticket.PasswordHash)
            return false;

        return await users.TryResetPasswordAsync(ticket.UserId, ticket.Email, ticket.PasswordHash,
            hasher.HashPassword(user, request.Password), cancellationToken);
    }
}
