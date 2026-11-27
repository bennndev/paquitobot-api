using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeProgramCourseSource(FrappeCourseCatalog catalog) : IFrappeSource<FrappeProgramCourseDto>
{
    public async Task<IReadOnlyList<FrappeProgramCourseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await catalog.GetAllAsync(cancellationToken);

        return courses
            .Where(course => !string.IsNullOrWhiteSpace(course.Program))
            .Select(course => new FrappeProgramCourseDto(course.Program!, course.Name, null, true))
            .ToList();
    }

}
