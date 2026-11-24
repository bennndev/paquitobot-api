using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Sync;

/// <summary>
/// Maps Frappe class slots to course schedules. The section is a required parent and the
/// instructor an optional one, both resolved by external id.
/// </summary>
public class CourseScheduleSynchronizer(
    IFrappeSource<FrappeCourseScheduleDto> source,
    IFrappeSyncedRepository<CourseSchedule> repository,
    IFrappeSyncedRepository<CourseSection> sections,
    IFrappeSyncedRepository<Instructor> instructors,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeCourseScheduleDto, CourseSchedule>(repository, unitOfWork, timeProvider)
{
    private IReadOnlyDictionary<string, CourseSection> _knownSections = new Dictionary<string, CourseSection>();
    private IReadOnlyDictionary<string, Instructor> _knownInstructors = new Dictionary<string, Instructor>();

    public override string EntityName => nameof(CourseSchedule);

    public override int Order => SyncOrder.SectionDetails;

    protected override Task<IReadOnlyList<FrappeCourseScheduleDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override async Task PrepareAsync(
        IReadOnlyList<FrappeCourseScheduleDto> items,
        CancellationToken cancellationToken)
    {
        _knownSections = await sections.GetByFrappeExternalIdsAsync(
            items.Select(i => i.SectionExternalId.Trim()), cancellationToken);

        _knownInstructors = await instructors.GetByFrappeExternalIdsAsync(
            items.Where(i => !string.IsNullOrWhiteSpace(i.InstructorExternalId)).Select(i => i.InstructorExternalId!.Trim()),
            cancellationToken);
    }

    protected override string GetExternalId(FrappeCourseScheduleDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeCourseScheduleDto external) => external.ModifiedAt;

    protected override CourseSchedule Create(FrappeCourseScheduleDto external)
    {
        var schedule = new CourseSchedule { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, schedule);

        return schedule;
    }

    protected override void Apply(FrappeCourseScheduleDto external, CourseSchedule schedule)
    {
        if (external.EndTime <= external.StartTime)
        {
            throw new InvalidOperationException("The end time must be after the start time.");
        }

        var sectionExternalId = external.SectionExternalId.Trim();

        if (!_knownSections.TryGetValue(sectionExternalId, out var section))
        {
            throw new InvalidOperationException($"Section '{sectionExternalId}' was not found.");
        }

        schedule.SectionId = section.SectionId;
        schedule.InstructorId = ResolveInstructor(external.InstructorExternalId);
        schedule.ClassDate = external.ClassDate;
        schedule.StartTime = external.StartTime;
        schedule.EndTime = external.EndTime;
        schedule.Location = TextNormalizer.Optional(external.Location);
        schedule.DeliveryMode = TextNormalizer.Optional(external.DeliveryMode);
        schedule.Status = TextNormalizer.Optional(external.Status);
    }

    private Guid? ResolveInstructor(string? instructorExternalId)
    {
        var id = TextNormalizer.Optional(instructorExternalId);

        if (id is null)
        {
            return null;
        }

        if (!_knownInstructors.TryGetValue(id, out var instructor))
        {
            throw new InvalidOperationException($"Instructor '{id}' was not found.");
        }

        return instructor.InstructorId;
    }
}
