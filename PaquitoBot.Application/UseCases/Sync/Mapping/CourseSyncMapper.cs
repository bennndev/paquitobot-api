using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class CourseSyncMapper
{
    public static void Apply(FrappeCourseDto source, Course course)
    {
        course.Code = TextNormalizer.Optional(source.Code);
        course.Name = TextNormalizer.Required(source.Name);
        course.Description = TextNormalizer.Optional(source.Description);
        course.Credits = source.Credits;

        if (!string.IsNullOrWhiteSpace(source.Status))
        {
            course.Status = TextNormalizer.OptionalLower(source.Status);
        }
    }
}
