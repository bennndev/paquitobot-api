
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.AcademicPeriods;
using PaquitoBot.Application.UseCases.AcademicPeriods.Command;
using PaquitoBot.Application.UseCases.AcademicPeriods.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/academic-periods")]
[Produces("application/json")]
public class AcademicPeriodsController : ControllerBase
{
    private readonly ISender _sender;

    public AcademicPeriodsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<AcademicPeriodDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AcademicPeriodDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var periods = await _sender.Send(
            new GetAllAcademicPeriodsQuery(),
            cancellationToken);

        return Ok(periods);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AcademicPeriodDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademicPeriodDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var period = await _sender.Send(
            new GetAcademicPeriodByIdQuery(id),
            cancellationToken);

        return Ok(period);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(AcademicPeriodDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademicPeriodDto>> Create(
        [FromBody] CreateAcademicPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var period = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = period.AcademicPeriodId },
            period);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(AcademicPeriodDto),
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
    public async Task<ActionResult<AcademicPeriodDto>> Update(
        Guid id,
        [FromBody] UpdateAcademicPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var period = await _sender.Send(
            command with { AcademicPeriodId = id },
            cancellationToken);

        return Ok(period);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
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
            new DeleteAcademicPeriodCommand(id),
            cancellationToken);

        return NoContent();
    }
}
