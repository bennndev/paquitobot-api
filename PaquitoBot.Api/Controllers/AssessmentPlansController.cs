using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.AssessmentPlans;
using PaquitoBot.Application.UseCases.AssessmentPlans.Command;
using PaquitoBot.Application.UseCases.AssessmentPlans.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/assessment-plans")]
public class AssessmentPlansController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssessmentPlanDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAllAssessmentPlansQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentPlanDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAssessmentPlanByIdQuery(id), cancellationToken));
    }

    [HttpGet("by-section/{sectionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentPlanDto>>> GetBySection(Guid sectionId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAssessmentPlansBySectionIdQuery(sectionId), cancellationToken));

    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<AssessmentPlanDto>>> GetUpcoming([FromQuery] Guid? sectionId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetUpcomingAssessmentPlansQuery(sectionId), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAssessmentPlanCommand command,
        CancellationToken cancellationToken)
    {
        var planId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = planId }, planId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] UpdateAssessmentPlanCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { AssessmentPlanId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAssessmentPlanCommand(id), cancellationToken);
        return NoContent();
    }
}
