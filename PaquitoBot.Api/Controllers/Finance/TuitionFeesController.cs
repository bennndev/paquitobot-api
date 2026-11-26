using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.TuitionFees;
using PaquitoBot.Application.UseCases.TuitionFees.Commands;
using PaquitoBot.Application.UseCases.TuitionFees.Queries;

namespace PaquitoBot.Api.Controllers.Finance;

[ApiController]
[Authorize(Policy = "Administrator")]
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
    public async Task<ActionResult> Create(
        [FromBody] CreateTuitionFeeCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Pensión creada" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateTuitionFeeCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { TuitionFeeId = id }, cancellationToken);
        return Ok(new { message = "Pensión actualizada" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTuitionFeeCommand(id), cancellationToken);

        return Ok(new { message = "Pensión eliminada" });
    }
}