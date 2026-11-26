using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.ProgramCourses;
using PaquitoBot.Application.UseCases.ProgramCourses.Commands;
using PaquitoBot.Application.UseCases.ProgramCourses.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/program-courses")]
public class ProgramCoursesController : ControllerBase
{
    private readonly ISender _sender;

    public ProgramCoursesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramCourseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllProgramCoursesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProgramCourseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetProgramCourseByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateProgramCourseCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return Ok(new { message = "Vínculo entre programa y curso creado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateProgramCourseCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { ProgramCourseId = id }, cancellationToken);
        return Ok(new { message = "Vínculo entre programa y curso actualizado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteProgramCourseCommand(id), cancellationToken);
        return Ok(new { message = "Vínculo entre programa y curso eliminado" });
    }
}
