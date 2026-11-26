
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.Programs;
using PaquitoBot.Application.UseCases.Programs.Commands;
using PaquitoBot.Application.UseCases.Programs.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/programs")]
public class ProgramsController : ControllerBase
{
    private readonly ISender _sender;

    public ProgramsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var programs = await _sender.Send(
            new GetAllProgramsQuery(),
            cancellationToken);

        return Ok(programs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProgramDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var program = await _sender.Send(
            new GetProgramByIdQuery(id),
            cancellationToken);

        return Ok(program);
    }

    [Authorize(Policy = "Administrator")]

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateProgramCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            command,
            cancellationToken);

        return Ok(new { message = "Programa creado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateProgramCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            command with { ProgramId = id },
            cancellationToken);

        return Ok(new { message = "Programa actualizado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteProgramCommand(id),
            cancellationToken);

        return Ok(new { message = "Programa eliminado" });
    }
}
