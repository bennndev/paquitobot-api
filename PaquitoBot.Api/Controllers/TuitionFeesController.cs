using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.TuitionFees;
using PaquitoBot.Application.UseCases.TuitionFees.Command;
using PaquitoBot.Application.UseCases.TuitionFees.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/tuition-fees")]
public class TuitionFeesController : ControllerBase
{
    private readonly ISender _sender;

    public TuitionFeesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TuitionFeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllTuitionFeesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TuitionFeeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetTuitionFeeByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<TuitionFeeDto>> Create(
        [FromBody] CreateTuitionFeeCommand command,
        CancellationToken cancellationToken)
    {
        var tuitionFee = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = tuitionFee.TuitionFeeId }, tuitionFee);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TuitionFeeDto>> Update(
        Guid id,
        [FromBody] UpdateTuitionFeeCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { TuitionFeeId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTuitionFeeCommand(id), cancellationToken);

        return NoContent();
    }
}