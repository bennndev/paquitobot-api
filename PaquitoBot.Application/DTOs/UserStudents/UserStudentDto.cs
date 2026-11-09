using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.DTOs.UserStudents;

public record UserStudentDto(
    Guid LinkId,
    Guid UserId,
    Guid StudentId,
    bool IsPrimary,
    DateTime CreatedAt)
{
    public static UserStudentDto From(UserStudent link)
    {
        return new UserStudentDto(
            link.LinkId,
            link.UserId,
            link.StudentId,
            link.IsPrimary,
            link.CreatedAt);
    }
}
