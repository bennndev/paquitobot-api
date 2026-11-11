using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.FeeStructures;
using PaquitoBot.Application.UseCases.FeeStructures.Command;
using PaquitoBot.Application.UseCases.FeeStructures.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
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
        return Ok(await _sender.Send(new GetAllFeeStructuresUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeeStructureDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetFeeStructureByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<FeeStructureDto>> Create(
        [FromBody] CreateFeeStructureUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var feeStructure = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = feeStructure.FeeStructureId }, feeStructure);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeeStructureDto>> Update(
        Guid id,
        [FromBody] UpdateFeeStructureUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { FeeStructureId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFeeStructureUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}