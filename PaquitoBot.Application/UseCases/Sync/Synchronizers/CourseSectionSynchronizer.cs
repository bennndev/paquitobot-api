using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

namespace PaquitoBot.Application.UseCases.Sync.Synchronizers;

/// <summary>
/// Maps Frappe batches to course sections. The course is a parent resolved by external id. The academic period
/// is the term reported by the batch enrollments; without one it is deduced as the single period that
/// contains the batch start date.
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
    private IReadOnlyDictionary<string, AcademicPeriod> _periodsByExternalId = new Dictionary<string, AcademicPeriod>();

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
        _periodsByExternalId = await periods.GetByFrappeExternalIdsAsync(
            items.Where(i => !string.IsNullOrWhiteSpace(i.AcademicPeriodExternalId)).Select(i => i.AcademicPeriodExternalId!.Trim()),
            cancellationToken);
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

        var period = AcademicPeriodResolver.Resolve(external.AcademicPeriodExternalId, external.StartDate,
            _periodsByExternalId, _knownPeriods);
        CourseSectionSyncMapper.Apply(external, section, course.CourseId, period.AcademicPeriodId);
    }
}
