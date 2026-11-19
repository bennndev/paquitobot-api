using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.DTOs.StudentFinances;
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

    private Guid? GetAuthenticatedUserId()
    {
        // JWT 'sub' is often mapped to ClaimTypes.NameIdentifier by JwtBearer.
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(subject, out var id) ? id : null;
    }
}
