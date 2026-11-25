using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.UseCases.StudentFinances.Command;
using PaquitoBot.Application.UseCases.StudentFinances.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/student-finances")]
[Produces("application/json")]
public sealed class StudentFinancesController(ISender sender) : ControllerBase
{
    [HttpGet("{studentId:guid}/statement")]
    [ProducesResponseType(typeof(StudentFinancialStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentFinancialStatementDto>> GetStatement(
        Guid studentId,
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null) return Unauthorized();

        var statement = await sender.Send(
            new GetStudentFinancialStatementQuery(studentId, userId.Value, asOf),
            cancellationToken);

        return Ok(statement);
    }

    [HttpGet("{studentId:guid}/payments")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetPayments(
        Guid studentId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null) return Unauthorized();

        var payments = await sender.Send(
            new GetStudentFinancialPaymentsQuery(studentId, userId.Value, status, from, to),
            cancellationToken);

        return Ok(payments);
    }

    // Catalog only: never aggregate catalog totals into individual student debt.
    [HttpGet("{studentId:guid}/schedules")]
    [ProducesResponseType(typeof(StudentFinancialSchedulesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentFinancialSchedulesDto>> GetSchedules(
        Guid studentId,
        [FromQuery] Guid? programId,
        [FromQuery] Guid? academicPeriodId,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null) return Unauthorized();

        var result = await sender.Send(
            new GetStudentFinancialSchedulesQuery(
                studentId, userId.Value, programId, academicPeriodId),
            cancellationToken);

        return Ok(result);
    }

    // Admin-only local balance audit: never fetches from Frappe and never modifies fees/payments.
    [HttpPost("{studentId:guid}/reconcile")]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(typeof(StudentFinancialReconciliationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentFinancialReconciliationDto>> Reconcile(
        Guid studentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RunStudentFinancialReconciliationCommand(studentId), cancellationToken);
        return Ok(result);
    }

    private Guid? GetAuthenticatedUserId()
    {
        // JWT 'sub' is often mapped to ClaimTypes.NameIdentifier by JwtBearer.
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(subject, out var id) ? id : null;
    }
}
