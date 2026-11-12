using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.ProgramCourses;

public record ProgramCourseDto(
    Guid ProgramCourseId,
    Guid ProgramId,
    Guid CourseId,
    int? CycleNumber,
    bool IsRequired,
    DateTime CreatedAt)
{
    public static ProgramCourseDto From(ProgramCourse programCourse)
    {
        return new ProgramCourseDto(
            programCourse.ProgramCourseId,
            programCourse.ProgramId,
            programCourse.CourseId,
            programCourse.CycleNumber,
            programCourse.IsRequired,
            programCourse.CreatedAt);
    }
}
