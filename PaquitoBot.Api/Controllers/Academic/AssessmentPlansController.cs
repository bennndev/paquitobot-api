using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.AssessmentPlans;
using PaquitoBot.Application.UseCases.AssessmentPlans.Commands;
using PaquitoBot.Application.UseCases.AssessmentPlans.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "Administrator")]
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
    public async Task<ActionResult> Create(
        [FromBody] CreateAssessmentPlanCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Ok(new { message = "Plan de evaluación creado" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateAssessmentPlanCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { AssessmentPlanId = id }, cancellationToken);
        return Ok(new { message = "Plan de evaluación actualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAssessmentPlanCommand(id), cancellationToken);
        return Ok(new { message = "Plan de evaluación eliminado" });
    }
}
