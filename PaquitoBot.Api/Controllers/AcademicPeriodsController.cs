
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.AcademicPeriods;
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
            new GetAllAcademicPeriodsUseCaseQuery(),
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
            new GetAcademicPeriodByIdUseCaseQuery(id),
            cancellationToken);

        return Ok(period);
    }
}
