using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.UseCases.Reports.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/reports")]
public sealed class AcademicReportsController(ISender sender, IReportExcelExporter exporter) : ControllerBase
{
    private const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("enrollments")]
    public async Task<ActionResult<IReadOnlyList<EnrollmentReportRowDto>>> Enrollments(
        [FromQuery] Guid? programId, [FromQuery] Guid? academicPeriodId,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetEnrollmentReportQuery(programId, academicPeriodId), cancellationToken));

    [HttpGet("attendance")]
    public async Task<ActionResult<IReadOnlyList<AttendanceReportRowDto>>> Attendance(
        [FromQuery] Guid? programId, [FromQuery] Guid? academicPeriodId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAttendanceReportQuery(programId, academicPeriodId, from, to), cancellationToken));

    [HttpGet("assessments")]
    public async Task<ActionResult<IReadOnlyList<AssessmentReportRowDto>>> Assessments(
        [FromQuery] Guid? programId, [FromQuery] Guid? academicPeriodId,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAssessmentReportQuery(programId, academicPeriodId), cancellationToken));

    [HttpGet("enrollments.xlsx")]
    public async Task<IActionResult> EnrollmentsExcel(
        [FromQuery] Guid? programId, [FromQuery] Guid? academicPeriodId,
        CancellationToken cancellationToken)
    {
        var rows = await sender.Send(new GetEnrollmentReportQuery(programId, academicPeriodId), cancellationToken);
        return File(exporter.ExportEnrollments(rows), Xlsx, FileName("enrollments"));
    }

    [HttpGet("attendance.xlsx")]
    public async Task<IActionResult> AttendanceExcel(
        [FromQuery] Guid? programId, [FromQuery] Guid? academicPeriodId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var rows = await sender.Send(
            new GetAttendanceReportQuery(programId, academicPeriodId, from, to), cancellationToken);
        return File(exporter.ExportAttendance(rows), Xlsx, FileName("attendance"));
    }

    [HttpGet("assessments.xlsx")]
    public async Task<IActionResult> AssessmentsExcel(
        [FromQuery] Guid? programId, [FromQuery] Guid? academicPeriodId,
        CancellationToken cancellationToken)
    {
        var rows = await sender.Send(new GetAssessmentReportQuery(programId, academicPeriodId), cancellationToken);
        return File(exporter.ExportAssessments(rows), Xlsx, FileName("assessments"));
    }

    private static string FileName(string report) => $"paquitobot-{report}-{DateTime.UtcNow:yyyyMMdd}.xlsx";
}
