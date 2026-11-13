namespace PaquitoBot.Application.Services;

public interface ITokenService
{
    string GenerateToken(Guid idUser, string role, string email);
}