using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Sync.Command;

namespace PaquitoBot.Api.Controllers;

/// <summary>
/// Manual synchronization of the academic master data owned by Frappe.
/// The result of the latest run per entity is exposed by <c>GET api/sync-runs/latest</c>.
/// </summary>
[ApiController]
[Route("api/academic-sync")]
[Authorize(Policy = "Administrator")]
public class AcademicSyncController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Runs a full synchronization in dependency order and reports, per entity,
    /// how many records were processed, created, updated, unchanged and failed.
    /// Records missing from Frappe are never deleted locally. Answers 409 if another run is in progress.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AcademicSyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademicSyncResponse>> Run(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new RunAcademicSyncCommand(), cancellationToken));
    }
}
