using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Sync.Command;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/academic-sync")]
[Authorize(Policy = "Administrator")]
public class AcademicSyncController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AcademicSyncResponse>> Run(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new RunAcademicSyncCommand(), cancellationToken));
    }
}
