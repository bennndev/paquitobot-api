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
        return Ok(await sender.Send(new GetAllAssessmentPlansUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentPlanDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAssessmentPlanByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAssessmentPlanUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var planId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = planId }, planId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] UpdateAssessmentPlanUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { AssessmentPlanId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAssessmentPlanUseCaseCommand(id), cancellationToken);
        return NoContent();
    }
}
