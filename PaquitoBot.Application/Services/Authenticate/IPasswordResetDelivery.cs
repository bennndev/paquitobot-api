namespace PaquitoBot.Application.Services.Authenticate;

public interface IPasswordResetDelivery
{
    bool IsConfigured { get; }
    string Protect(Guid userId, string email, string passwordHash);
    PasswordResetTicket? Unprotect(string token);
    Task SendAsync(string email, string token, CancellationToken cancellationToken);
}

public sealed record PasswordResetTicket(Guid UserId, string Email, string PasswordHash);

public interface IPasswordResetQueue
{
    bool TryEnqueue(string email);
}
