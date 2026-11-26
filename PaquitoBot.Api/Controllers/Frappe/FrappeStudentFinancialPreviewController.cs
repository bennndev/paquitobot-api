using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.StudentFinances;
using PaquitoBot.Application.UseCases.StudentFinances.Commands;

namespace PaquitoBot.Api.Controllers.Frappe;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/student-finances")]
public sealed class FrappeStudentFinancialPreviewController(ISender sender) : ControllerBase
{
    [HttpPost("{studentId:guid}/frappe/preview")]
    public async Task<ActionResult<FrappeFinancialPreviewDto>> Preview(
        Guid studentId,
        [FromQuery] string source = "Fees",
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new PreviewFrappeStudentFeesCommand(studentId, source),
            cancellationToken);

        return Ok(result);
    }
}
