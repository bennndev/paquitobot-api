namespace PaquitoBot.Application.DTOs.Users;

public record CreateUserRequest(
    string Username,
    string Email,
    string Role,
    bool IsActive = true);
