namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeProgramCourseDto(
    string ProgramExternalId,
    string CourseExternalId,
    int? CycleNumber,
    bool IsRequired);
