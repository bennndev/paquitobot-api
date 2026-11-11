using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.UseCases.ProgramEnrollments.Command;
using PaquitoBot.Application.UseCases.ProgramEnrollments.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/program-enrollments")]
[Produces("application/json")]
public class ProgramEnrollmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ProgramEnrollmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProgramEnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProgramEnrollmentDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllProgramEnrollmentsUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProgramEnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramEnrollmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetProgramEnrollmentByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgramEnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgramEnrollmentDto>> Create(
        [FromBody] CreateProgramEnrollmentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var enrollment = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = enrollment.ProgramEnrollmentId },
            enrollment);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProgramEnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgramEnrollmentDto>> Update(
        Guid id,
        [FromBody] UpdateProgramEnrollmentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(
            command with { ProgramEnrollmentId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteProgramEnrollmentUseCaseCommand(id), cancellationToken);
        return NoContent();
    }
}
