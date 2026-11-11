using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.FeeSchedules;

public record FeeScheduleDto(
    Guid FeeScheduleId,
    string? FrappeExternalId,
    Guid FeeStructureId,
    Guid? AcademicPeriodId,
    string Name,
    DateOnly? DueDate,
    decimal? ScheduledAmount,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static FeeScheduleDto From(FeeSchedule feeSchedule)
    {
        return new FeeScheduleDto(
            feeSchedule.FeeScheduleId,
            feeSchedule.FrappeExternalId,
            feeSchedule.FeeStructureId,
            feeSchedule.AcademicPeriodId,
            feeSchedule.Name,
            feeSchedule.DueDate,
            feeSchedule.ScheduledAmount,
            feeSchedule.Status,
            feeSchedule.FrappeModifiedAt,
            feeSchedule.SyncedAt,
            feeSchedule.CreatedAt,
            feeSchedule.UpdatedAt);
    }
}