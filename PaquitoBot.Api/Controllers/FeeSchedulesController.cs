using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.FeeSchedules;
using PaquitoBot.Application.UseCases.FeeSchedules.Command;
using PaquitoBot.Application.UseCases.FeeSchedules.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/fee-schedules")]
public class FeeSchedulesController : ControllerBase
{
    private readonly ISender _sender;

    public FeeSchedulesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeeScheduleDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllFeeSchedulesUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeeScheduleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetFeeScheduleByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<FeeScheduleDto>> Create(
        [FromBody] CreateFeeScheduleUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var feeSchedule = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = feeSchedule.FeeScheduleId }, feeSchedule);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeeScheduleDto>> Update(
        Guid id,
        [FromBody] UpdateFeeScheduleUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { FeeScheduleId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFeeScheduleUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}