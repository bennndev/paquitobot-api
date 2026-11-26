namespace PaquitoBot.Application.Dtos.Frappe;

public record FrappeProgramCourseDto(
    string ProgramExternalId,
    string CourseExternalId,
    int? CycleNumber,
    bool IsRequired);
