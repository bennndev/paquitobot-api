namespace PaquitoBot.Application.Services.Authenticate;

public interface ITokenService
{
    string GenerateToken(Guid idUser, string role, string email, Guid? studentId = null);
}
