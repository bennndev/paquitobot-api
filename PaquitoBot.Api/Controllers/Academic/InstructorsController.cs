using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.Instructors;
using PaquitoBot.Application.UseCases.Instructors.Commands;
using PaquitoBot.Application.UseCases.Instructors.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/instructors")]
public class InstructorsController : ControllerBase
{
    private readonly ISender _sender;

    public InstructorsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstructorDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllInstructorsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetInstructorByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateInstructorCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Instructor creado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateInstructorCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { InstructorId = id }, cancellationToken);
        return Ok(new { message = "Instructor actualizado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteInstructorCommand(id), cancellationToken);

        return Ok(new { message = "Instructor eliminado" });
    }
}
