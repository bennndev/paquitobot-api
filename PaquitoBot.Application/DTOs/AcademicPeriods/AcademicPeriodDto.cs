
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.AcademicPeriods;

public record AcademicPeriodDto(
    Guid AcademicPeriodId,
    string? FrappeExternalId,
    string? Code,
    string Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static AcademicPeriodDto From(AcademicPeriod period)
    {
        return new AcademicPeriodDto(
            period.AcademicPeriodId,
            period.FrappeExternalId,
            period.Code,
            period.Name,
            period.StartDate,
            period.EndDate,
            period.Status,
            period.FrappeModifiedAt,
            period.SyncedAt,
            period.CreatedAt,
            period.UpdatedAt);
    }
}
