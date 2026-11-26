using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.Payments;
using PaquitoBot.Application.Dtos.StudentFinances;
using PaquitoBot.Application.UseCases.StudentFinances.Commands;
using PaquitoBot.Application.UseCases.StudentFinances.Queries;

namespace PaquitoBot.Api.Controllers.Finance;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/student-finances")]
public sealed class StudentFinancesController(ISender sender) : ControllerBase
{
    [HttpGet("{studentId:guid}/statement")]
    public async Task<ActionResult<StudentFinancialStatementDto>> GetStatement(
        Guid studentId,
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        var statement = await sender.Send(
            new GetStudentFinancialStatementQuery(studentId, asOf),
            cancellationToken);

        return Ok(statement);
    }

    [HttpGet("{studentId:guid}/payments")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetPayments(
        Guid studentId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var payments = await sender.Send(
            new GetStudentFinancialPaymentsQuery(studentId, status, from, to),
            cancellationToken);

        return Ok(payments);
    }

    [HttpGet("{studentId:guid}/schedules")]
    public async Task<ActionResult<StudentFinancialSchedulesDto>> GetSchedules(
        Guid studentId,
        [FromQuery] Guid? programId,
        [FromQuery] Guid? academicPeriodId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetStudentFinancialSchedulesQuery(
                studentId, programId, academicPeriodId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{studentId:guid}/reconcile")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult<StudentFinancialReconciliationDto>> Reconcile(
        Guid studentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RunStudentFinancialReconciliationCommand(studentId), cancellationToken);
        return Ok(result);
    }
}
