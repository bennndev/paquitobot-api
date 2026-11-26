
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.AcademicPeriods;
using PaquitoBot.Application.UseCases.AcademicPeriods.Commands;
using PaquitoBot.Application.UseCases.AcademicPeriods.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/academic-periods")]
public class AcademicPeriodsController : ControllerBase
{
    private readonly ISender _sender;

    public AcademicPeriodsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AcademicPeriodDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var periods = await _sender.Send(
            new GetAllAcademicPeriodsQuery(),
            cancellationToken);

        return Ok(periods);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AcademicPeriodDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var period = await _sender.Send(
            new GetAcademicPeriodByIdQuery(id),
            cancellationToken);

        return Ok(period);
    }

    [Authorize(Policy = "Administrator")]

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateAcademicPeriodCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            command,
            cancellationToken);

        return Ok(new { message = "Periodo académico creado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateAcademicPeriodCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            command with { AcademicPeriodId = id },
            cancellationToken);

        return Ok(new { message = "Periodo académico actualizado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteAcademicPeriodCommand(id),
            cancellationToken);

        return Ok(new { message = "Periodo académico eliminado" });
    }
}
