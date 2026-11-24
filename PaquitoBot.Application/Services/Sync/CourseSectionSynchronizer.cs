using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Sync;

/// <summary>
/// Maps Frappe batches to course sections. The course is a parent resolved by external id;
/// the academic period is deduced as the single period that contains the batch start date.
/// </summary>
public class CourseSectionSynchronizer(
    IFrappeSource<FrappeCourseSectionDto> source,
    IFrappeSyncedRepository<CourseSection> repository,
    IFrappeSyncedRepository<Course> courses,
    IFrappeSyncedRepository<AcademicPeriod> periods,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeCourseSectionDto, CourseSection>(repository, unitOfWork, timeProvider)
{
    private IReadOnlyDictionary<string, Course> _knownCourses = new Dictionary<string, Course>();
    private IReadOnlyList<AcademicPeriod> _knownPeriods = [];

    public override string EntityName => nameof(CourseSection);

    public override int Order => SyncOrder.Sections;

    protected override Task<IReadOnlyList<FrappeCourseSectionDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override async Task PrepareAsync(
        IReadOnlyList<FrappeCourseSectionDto> items,
        CancellationToken cancellationToken)
    {
        _knownCourses = await courses.GetByFrappeExternalIdsAsync(
            items.Select(i => i.CourseExternalId.Trim()), cancellationToken);
        _knownPeriods = await periods.GetAllAsync(cancellationToken);
    }

    protected override string GetExternalId(FrappeCourseSectionDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeCourseSectionDto external) => external.ModifiedAt;

    protected override CourseSection Create(FrappeCourseSectionDto external)
    {
        var section = new CourseSection { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, section);

        return section;
    }

    protected override void Apply(FrappeCourseSectionDto external, CourseSection section)
    {
        var courseExternalId = external.CourseExternalId.Trim();

        if (!_knownCourses.TryGetValue(courseExternalId, out var course))
        {
            throw new InvalidOperationException($"Course '{courseExternalId}' was not found.");
        }

        section.CourseId = course.CourseId;
        section.AcademicPeriodId = ResolvePeriod(external).AcademicPeriodId;
        section.SectionCode = TextNormalizer.Optional(external.SectionCode);
        section.InstructorName = TextNormalizer.Optional(external.InstructorName);
        section.DeliveryMode = TextNormalizer.Optional(external.DeliveryMode);
        section.Capacity = external.Capacity;
        section.Status = TextNormalizer.Optional(external.Status);
    }

    private AcademicPeriod ResolvePeriod(FrappeCourseSectionDto external)
    {
        if (external.StartDate is not { } start)
        {
            throw new InvalidOperationException(
                "The batch has no start date, so its academic period cannot be deduced.");
        }

        var matches = _knownPeriods
            .Where(p => p.StartDate <= start && start <= p.EndDate)
            .ToList();

        return matches.Count switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException($"No academic period contains the batch start date {start:yyyy-MM-dd}."),
            _ => throw new InvalidOperationException(
                $"More than one academic period contains the batch start date {start:yyyy-MM-dd}.")
        };
    }
}
