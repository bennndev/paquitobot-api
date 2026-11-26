using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.UseCases.StudentFinances.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/reports/finances")]
public sealed class FinancialReportsController(
    ISender sender, ICurrentUser currentUser, IReportExcelExporter exporter) : ControllerBase
{
    private const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("{studentId:guid}")]
    public async Task<ActionResult<StudentFinancialStatementDto>> Get(
        Guid studentId, [FromQuery] DateOnly? asOf, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await sender.Send(new GetStudentFinancialStatementQuery(studentId, userId, asOf), cancellationToken));
    }

    [HttpGet("{studentId:guid}.xlsx")]
    public async Task<IActionResult> Excel(
        Guid studentId, [FromQuery] DateOnly? asOf, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var statement = await sender.Send(
            new GetStudentFinancialStatementQuery(studentId, userId, asOf), cancellationToken);
        return File(exporter.ExportStudentFinance(statement), Xlsx,
            $"paquitobot-finance-{studentId:N}-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
