using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.DTOs.Users;

public record UserDto(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static UserDto From(User user)
    {
        return new UserDto(
            user.UserId,
            user.Username,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
