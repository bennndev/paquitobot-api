using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class AcademicPeriodSyncMapper
{
    public static void Apply(FrappeAcademicPeriodDto source, AcademicPeriod period)
    {
        period.Code = TextNormalizer.Optional(source.Code);
        period.Name = TextNormalizer.Required(source.Name);
        period.StartDate = source.StartDate;
        period.EndDate = source.EndDate;

        if (!string.IsNullOrWhiteSpace(source.Status))
        {
            period.Status = TextNormalizer.OptionalLower(source.Status);
        }
    }
}
