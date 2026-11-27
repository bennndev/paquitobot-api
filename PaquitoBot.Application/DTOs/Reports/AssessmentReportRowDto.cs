namespace PaquitoBot.Application.DTOs.Reports;

public sealed record AssessmentReportRowDto(
    Guid SectionId,
    string CourseName,
    string SectionCode,
    int GradedResults,
    decimal AverageScorePercent);
