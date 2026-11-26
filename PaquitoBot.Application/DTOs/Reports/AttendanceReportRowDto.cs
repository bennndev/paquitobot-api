namespace PaquitoBot.Application.DTOs.Reports;

public sealed record AttendanceReportRowDto(
    Guid SectionId,
    string CourseName,
    string SectionCode,
    int Records,
    int Present,
    int Absent,
    int Late,
    int Excused,
    int Other)
{
    // Excused and unknown status rows are excluded from the denominator.
    public decimal? AttendancePercent => Present + Late + Absent == 0
        ? null
        : Math.Round(100m * (Present + Late) / (Present + Late + Absent), 2);
}
