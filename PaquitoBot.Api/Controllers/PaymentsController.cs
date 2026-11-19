using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.UseCases.Payments.Command;
using PaquitoBot.Application.UseCases.Payments.Query;

namespace PaquitoBot.Api.Controllers;

// Global financial payment administration is restricted to administrators.
// PUT/DELETE routes are intentionally absent until audited reversal is implemented.
// The finance ledger must not allow untracked edits or physical deletion of payments.
[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllPaymentsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetPaymentByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(
        [FromBody] CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.PaymentId }, payment);
    }
}
