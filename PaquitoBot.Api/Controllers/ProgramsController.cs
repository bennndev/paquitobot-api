
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Programs;
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
            new GetAllProgramsUseCaseQuery(),
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
            new GetProgramByIdUseCaseQuery(id),
            cancellationToken);

        return Ok(program);
    }
}
