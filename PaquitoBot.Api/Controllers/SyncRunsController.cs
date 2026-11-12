using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.SyncRuns.Command;
using PaquitoBot.Application.UseCases.SyncRuns.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/sync-runs")]
public class SyncRunsController : ControllerBase
{
    private readonly ISender _sender;

    public SyncRunsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetAllSyncRunsResponse>>> GetAll(
        [FromQuery] string? entityName,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllSyncRunsQuery(entityName, status), cancellationToken));
    }

    [HttpGet("latest")]
    public async Task<ActionResult<IReadOnlyList<GetLatestSyncRunByEntityResponse>>> GetLatestByEntity(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetLatestSyncRunByEntityQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetSyncRunByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetSyncRunByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Start(
        [FromBody] StartSyncRunCommand command,
        CancellationToken cancellationToken)
    {
        var syncRunId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = syncRunId }, syncRunId);
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteSyncRunCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { SyncRunId = id }, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteSyncRunCommand(id), cancellationToken);

        return NoContent();
    }
}
