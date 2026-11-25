using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Sync;

public class AcademicPeriodSynchronizer(
    IFrappeSource<FrappeAcademicPeriodDto> source,
    IFrappeSyncedRepository<AcademicPeriod> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeAcademicPeriodDto, AcademicPeriod>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => nameof(AcademicPeriod);

    public override int Order => SyncOrder.MasterData;

    protected override Task<IReadOnlyList<FrappeAcademicPeriodDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override string GetExternalId(FrappeAcademicPeriodDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeAcademicPeriodDto external) => external.ModifiedAt;

    protected override AcademicPeriod Create(FrappeAcademicPeriodDto external)
    {
        var entity = new AcademicPeriod { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, entity);

        return entity;
    }

    protected override void Apply(FrappeAcademicPeriodDto external, AcademicPeriod entity)
    {
        entity.Code = TextNormalizer.Optional(external.Code);
        entity.Name = TextNormalizer.Required(external.Name);
        entity.StartDate = external.StartDate;
        entity.EndDate = external.EndDate;
        entity.Status = TextNormalizer.OptionalLower(external.Status);
    }
}
