using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.ProgramEnrollments;
using PaquitoBot.Application.UseCases.ProgramEnrollments.Commands;
using PaquitoBot.Application.UseCases.ProgramEnrollments.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/program-enrollments")]
public class ProgramEnrollmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ProgramEnrollmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramEnrollmentDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllProgramEnrollmentsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProgramEnrollmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetProgramEnrollmentByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateProgramEnrollmentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Matrícula creada" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateProgramEnrollmentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            command with { ProgramEnrollmentId = id }, cancellationToken);
        return Ok(new { message = "Matrícula actualizada" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteProgramEnrollmentCommand(id), cancellationToken);
        return Ok(new { message = "Matrícula eliminada" });
    }
}
