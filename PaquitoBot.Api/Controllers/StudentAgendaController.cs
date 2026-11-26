using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.StudentAgenda.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/student-agenda")]
public sealed class StudentAgendaController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<MyAcademicAgenda>> GetMine(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetMyAcademicAgendaQuery(from, to), cancellationToken));
    }
}
