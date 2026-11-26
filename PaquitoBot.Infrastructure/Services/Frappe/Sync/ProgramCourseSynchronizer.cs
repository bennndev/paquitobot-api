using PaquitoBot.Application.Dtos.Sync;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Services.Frappe.Sync;

public class ProgramCourseSynchronizer(
    IFrappeSource<FrappeProgramCourseDto> source,
    IFrappeSyncedRepository<Program> programs,
    IFrappeSyncedRepository<Course> courses,
    IGenericRepository<ProgramCourse> programCourses,
    IUnitOfWork unitOfWork) : IEntitySynchronizer
{
    public string EntityName => nameof(ProgramCourse);

    public int Order => SyncOrder.ProgramStructure;

    public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var items = await source.GetAllAsync(cancellationToken);

        var knownPrograms = await programs.GetByFrappeExternalIdsAsync(
            items.Select(i => i.ProgramExternalId.Trim()), cancellationToken);
        var knownCourses = await courses.GetByFrappeExternalIdsAsync(
            items.Select(i => i.CourseExternalId.Trim()), cancellationToken);

        var links = (await programCourses.GetAllAsync(cancellationToken))
            .Select(link => (link.ProgramId, link.CourseId))
            .ToHashSet();

        int created = 0, unchanged = 0;
        var errors = new List<SyncError>();

        foreach (var item in items)
        {
            var programId = item.ProgramExternalId.Trim();
            var courseId = item.CourseExternalId.Trim();

            if (!knownPrograms.TryGetValue(programId, out var program))
            {
                errors.Add(new SyncError(courseId, $"Program '{programId}' was not found."));
                continue;
            }

            if (!knownCourses.TryGetValue(courseId, out var course))
            {
                errors.Add(new SyncError(courseId, $"Course '{courseId}' was not found."));
                continue;
            }

            if (!links.Add((program.ProgramId, course.CourseId)))
            {
                unchanged++;
                continue;
            }

            await programCourses.AddAsync(new ProgramCourse
            {
                ProgramId = program.ProgramId,
                CourseId = course.CourseId,
                CycleNumber = item.CycleNumber,
                IsRequired = item.IsRequired
            }, cancellationToken);
            created++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SyncResult(EntityName, created, 0, unchanged, errors);
    }
}
