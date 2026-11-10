
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.Programs;

public record ProgramDto(
    Guid ProgramId,
    string FrappeExternalId,
    string? Code,
    string Name,
    string? Description,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ProgramDto From(Program program)
    {
        return new ProgramDto(
            program.ProgramId,
            program.FrappeExternalId,
            program.Code,
            program.Name,
            program.Description,
            program.Status,
            program.FrappeModifiedAt,
            program.SyncedAt,
            program.CreatedAt,
            program.UpdatedAt);
    }
}
