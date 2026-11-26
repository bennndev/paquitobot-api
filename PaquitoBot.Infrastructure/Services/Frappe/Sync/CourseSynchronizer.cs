using PaquitoBot.Application.Dtos.Sync;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Services.Frappe.Sync;

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
        entity.Code = TextNormalizer.Optional(external.Code);
        entity.Name = TextNormalizer.Required(external.Name);
        entity.Description = TextNormalizer.Optional(external.Description);
        entity.Credits = external.Credits;
        entity.Status = TextNormalizer.OptionalLower(external.Status);
    }
}
