using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.FeeSchedules;
using PaquitoBot.Application.UseCases.FeeSchedules.Commands;
using PaquitoBot.Application.UseCases.FeeSchedules.Queries;

namespace PaquitoBot.Api.Controllers.Finance;

[ApiController]
[Authorize(Policy = "Administrator")]
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
        return Ok(await _sender.Send(new GetAllFeeSchedulesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeeScheduleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetFeeScheduleByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateFeeScheduleCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Cronograma de pago creado" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateFeeScheduleCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { FeeScheduleId = id }, cancellationToken);
        return Ok(new { message = "Cronograma de pago actualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFeeScheduleCommand(id), cancellationToken);

        return Ok(new { message = "Cronograma de pago eliminado" });
    }
}