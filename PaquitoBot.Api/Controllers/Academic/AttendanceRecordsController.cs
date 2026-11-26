using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.AttendanceRecords;
using PaquitoBot.Application.UseCases.AttendanceRecords.Commands;
using PaquitoBot.Application.UseCases.AttendanceRecords.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/attendance-records")]
public class AttendanceRecordsController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetMyAttendanceQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAllAttendanceRecordsQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttendanceRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAttendanceRecordByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-student/{studentId:guid}/section/{sectionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetByStudentAndSection(
        Guid studentId, Guid sectionId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAttendanceByStudentAndSectionQuery(studentId, sectionId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateAttendanceRecordCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Ok(new { message = "Registro de asistencia creado" });
    }

    [Authorize(Policy = "Administrator")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateAttendanceRecordCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { AttendanceId = id }, cancellationToken);
        return Ok(new { message = "Registro de asistencia actualizado" });
    }

    [Authorize(Policy = "Administrator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAttendanceRecordCommand(id), cancellationToken);
        return Ok(new { message = "Registro de asistencia eliminado" });
    }
}
