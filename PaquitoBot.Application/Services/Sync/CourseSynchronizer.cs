using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

namespace PaquitoBot.Application.Services.Sync;

public class CourseSynchronizer(
    IFrappeSource<FrappeCourseDto> source,
    IFrappeSyncedRepository<Course> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeCourseDto, Course>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => nameof(Course);

    public override int Order => SyncOrder.MasterData;

    protected override Task<IReadOnlyList<FrappeCourseDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override string GetExternalId(FrappeCourseDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeCourseDto external) => external.ModifiedAt;

    protected override Course Create(FrappeCourseDto external)
    {
        var entity = new Course { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, entity);

        return entity;
    }

    protected override void Apply(FrappeCourseDto external, Course entity)
    {
        CourseSyncMapper.Apply(external, entity);
    }
}
