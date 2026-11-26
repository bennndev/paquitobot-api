using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.AssessmentsResults;
using PaquitoBot.Application.UseCases.AssessmentsResults.Commands;
using PaquitoBot.Application.UseCases.AssessmentsResults.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/assessment-results")]
public class AssessmentResultsController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AssessmentResultDto>>> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetMyAssessmentResultsQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssessmentResultDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAllAssessmentResultsQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentResultDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAssessmentResultByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-assessment/{assessmentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentResultDto>>> GetByAssessment(Guid assessmentId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetResultsByAssessmentIdQuery(assessmentId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentResultDto>>> GetByStudent(Guid studentId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetResultsByStudentIdQuery(studentId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-student/{studentId:guid}/assessment/{assessmentId:guid}")]
    public async Task<ActionResult<AssessmentResultDto>> GetStudentResultByAssessment(
        Guid studentId, Guid assessmentId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetStudentResultByAssessmentIdQuery(studentId, assessmentId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateAssessmentResultCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Ok(new { message = "Resultado de evaluación creado" });
    }

    [Authorize(Policy = "Administrator")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateAssessmentResultCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { ResultId = id }, cancellationToken);
        return Ok(new { message = "Resultado de evaluación actualizado" });
    }

    [Authorize(Policy = "Administrator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAssessmentResultCommand(id), cancellationToken);
        return Ok(new { message = "Resultado de evaluación eliminado" });
    }
}
