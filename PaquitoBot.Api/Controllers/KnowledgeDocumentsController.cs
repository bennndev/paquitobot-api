using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.KnowledgeDocuments.Command;
using PaquitoBot.Application.UseCases.KnowledgeDocuments.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/knowledge-documents")]
public class KnowledgeDocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public KnowledgeDocumentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetAllKnowledgeDocumentsResponse>>> GetAll(
        [FromQuery] Guid? courseId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllKnowledgeDocumentsUseCaseQuery(courseId, status, search, page, pageSize);

        return Ok(await _sender.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetKnowledgeDocumentByIdResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetKnowledgeDocumentByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateKnowledgeDocumentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var documentId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = documentId }, documentId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateKnowledgeDocumentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { DocumentId = id }, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/indexed")]
    public async Task<IActionResult> MarkAsIndexed(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new MarkKnowledgeDocumentAsIndexedUseCaseCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteKnowledgeDocumentUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}
