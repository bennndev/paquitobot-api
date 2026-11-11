using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.AssessmentsResults;

public record AssessmentResultDto(
    Guid ResultId,
    string? FrappeExternalId,
    Guid AssessmentId,
    Guid StudentId,
    decimal? Grade,
    string? Notes,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static AssessmentResultDto From(AssessmentResult result)
    {
        return new AssessmentResultDto(
            result.ResultId,
            result.FrappeExternalId,
            result.AssessmentId,
            result.StudentId,
            result.Grade,
            result.Notes,
            result.FrappeModifiedAt,
            result.SyncedAt,
            result.CreatedAt,
            result.UpdatedAt
        );
    }
}
