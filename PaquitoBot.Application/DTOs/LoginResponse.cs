namespace PaquitoBot.Application.DTOs;

public record LoginResponse(
    Guid UserId,
    string Token,
    string Email,
    string Role
)
{
    public static LoginResponse From(
        Guid  userId,
        string token,
        string email,
        string role)
    {
        return new LoginResponse
        (
            userId,
            token,
            email,
            role
        );
    }
};
