namespace PaquitoBot.Application.DTOs.Users;

public record UpdateUserRequest(
    string Username,
    string Email,
    string Role,
    bool IsActive);
