using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.Assessments;

public record AssessmentDto(
    Guid AssessmentId,
    string? FrappeExternalId,
    Guid SectionId,
    string Title,
    string? AssessmentType,
    decimal? MaximumScore,
    decimal? Weight,
    DateOnly? AssessmentDate,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static AssessmentDto From(Assessment assessment)
    {
        return new AssessmentDto(
            assessment.AssessmentId,
            assessment.FrappeExternalId,
            assessment.SectionId,
            assessment.Title,
            assessment.AssessmentType,
            assessment.MaximumScore,
            assessment.Weight,
            assessment.AssessmentDate,
            assessment.Status,
            assessment.FrappeModifiedAt,
            assessment.SyncedAt,
            assessment.CreatedAt,
            assessment.UpdatedAt
        );
    }
}