namespace PaquitoBot.Application.Frappe.Dtos;

public record FrappeProgramCourseDto(
    string ProgramExternalId,
    string CourseExternalId,
    int? CycleNumber,
    bool IsRequired);
