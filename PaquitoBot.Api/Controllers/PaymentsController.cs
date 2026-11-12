using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.UseCases.Payments.Command;
using PaquitoBot.Application.UseCases.Payments.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> Update(
        Guid id,
        [FromBody] UpdatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { PaymentId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePaymentCommand(id), cancellationToken);

        return NoContent();
    }
}