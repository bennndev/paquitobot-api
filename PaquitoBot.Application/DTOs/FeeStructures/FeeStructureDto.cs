using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.FeeStructures;

public record FeeStructureDto(
    Guid FeeStructureId,
    string? FrappeExternalId,
    Guid? ProgramId,
    string Name,
    string? Description,
    string CurrencyCode,
    decimal? TotalAmount,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static FeeStructureDto From(FeeStructure feeStructure)
    {
        return new FeeStructureDto(
            feeStructure.FeeStructureId,
            feeStructure.FrappeExternalId,
            feeStructure.ProgramId,
            feeStructure.Name,
            feeStructure.Description,
            feeStructure.CurrencyCode,
            feeStructure.TotalAmount,
            feeStructure.Status,
            feeStructure.FrappeModifiedAt,
            feeStructure.SyncedAt,
            feeStructure.CreatedAt,
            feeStructure.UpdatedAt);
    }
}