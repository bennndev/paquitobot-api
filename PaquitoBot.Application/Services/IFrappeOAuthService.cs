namespace PaquitoBot.Application.Services;

public record FrappeAuthorization(string Url);

public record FrappeAuthorizationResult(Guid UserId, string Email, string AccessToken);

public interface IFrappeOAuthService
{
    FrappeAuthorization Begin(Guid userId, string email);

    Task<FrappeAuthorizationResult> CompleteAsync(string code, string state, CancellationToken cancellationToken);
}
