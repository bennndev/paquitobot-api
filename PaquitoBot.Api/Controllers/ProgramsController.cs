
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Programs;
using PaquitoBot.Application.UseCases.Programs.Command;
using PaquitoBot.Application.UseCases.Programs.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/programs")]
[Produces("application/json")]
public class ProgramsController : ControllerBase
{
    private readonly ISender _sender;

    public ProgramsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProgramDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProgramDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var programs = await _sender.Send(
            new GetAllProgramsQuery(),
            cancellationToken);

        return Ok(programs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ProgramDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var program = await _sender.Send(
            new GetProgramByIdQuery(id),
            cancellationToken);

        return Ok(program);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ProgramDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgramDto>> Create(
        [FromBody] CreateProgramCommand command,
        CancellationToken cancellationToken)
    {
        var program = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = program.ProgramId },
            program);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(ProgramDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgramDto>> Update(
        Guid id,
        [FromBody] UpdateProgramCommand command,
        CancellationToken cancellationToken)
    {
        var program = await _sender.Send(
            command with { ProgramId = id },
            cancellationToken);

        return Ok(program);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteProgramCommand(id),
            cancellationToken);

        return NoContent();
    }
}
