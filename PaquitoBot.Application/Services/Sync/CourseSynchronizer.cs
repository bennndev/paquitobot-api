using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Sync;

public class CourseSynchronizer(
    IFrappeClient client,
    IFrappeSyncedRepository<Course> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeCourseDto, Course>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => nameof(Course);

    public override int Order => SyncOrder.MasterData;

    protected override Task<IReadOnlyList<FrappeCourseDto>> FetchAsync(CancellationToken cancellationToken)
        => client.GetCoursesAsync(cancellationToken);

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
        entity.Code = TextNormalizer.Optional(external.Code);
        entity.Name = TextNormalizer.Required(external.Name);
        entity.Description = TextNormalizer.Optional(external.Description);
        entity.Credits = external.Credits;
        entity.Status = TextNormalizer.Optional(external.Status);
    }
}
