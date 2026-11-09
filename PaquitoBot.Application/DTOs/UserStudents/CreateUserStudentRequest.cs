namespace PaquitoBot.Application.DTOs.UserStudents;

public record CreateUserStudentRequest(
    Guid UserId,
    Guid StudentId,
    bool IsPrimary = true);
