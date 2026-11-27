using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeCourseSource(FrappeCourseCatalog catalog) : IFrappeSource<FrappeCourseDto>
{
    public async Task<IReadOnlyList<FrappeCourseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await catalog.GetAllAsync(cancellationToken);
        return courses.Select(course => new FrappeCourseDto(
            course.Name, course.Abbreviation, course.CourseName ?? course.Name, course.ShortDescription,
            course.Credit is null ? null : (int)Math.Round(course.Credit.Value), null,
            FrappeFormats.ParseTimestamp(course.Modified))).ToList();
    }
}
