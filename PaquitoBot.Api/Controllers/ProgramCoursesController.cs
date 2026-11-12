using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.ProgramCourses;
using PaquitoBot.Application.UseCases.ProgramCourses.Command;
using PaquitoBot.Application.UseCases.ProgramCourses.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/program-courses")]
[Produces("application/json")]
public class ProgramCoursesController : ControllerBase
{
    private readonly ISender _sender;

    public ProgramCoursesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProgramCourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProgramCourseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllProgramCoursesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProgramCourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramCourseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetProgramCourseByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgramCourseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgramCourseDto>> Create(
        [FromBody] CreateProgramCourseCommand command,
        CancellationToken cancellationToken)
    {
        var link = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = link.ProgramCourseId }, link);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProgramCourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgramCourseDto>> Update(
        Guid id,
        [FromBody] UpdateProgramCourseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { ProgramCourseId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteProgramCourseCommand(id), cancellationToken);
        return NoContent();
    }
}
