using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.FeeStructures;
using PaquitoBot.Application.UseCases.FeeStructures.Commands;
using PaquitoBot.Application.UseCases.FeeStructures.Queries;

namespace PaquitoBot.Api.Controllers.Finance;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/fee-structures")]
public class FeeStructuresController : ControllerBase
{
    private readonly ISender _sender;

    public FeeStructuresController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeeStructureDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllFeeStructuresQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeeStructureDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetFeeStructureByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateFeeStructureCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Estructura de tarifas creada" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateFeeStructureCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { FeeStructureId = id }, cancellationToken);
        return Ok(new { message = "Estructura de tarifas actualizada" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFeeStructureCommand(id), cancellationToken);

        return Ok(new { message = "Estructura de tarifas eliminada" });
    }
}