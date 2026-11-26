using MediatR;
using PaquitoBot.Application.Dtos.Payments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Queries;

public record GetAllPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

internal sealed class GetAllPaymentsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetAllPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.Repository<Payment>().GetAllAsync(cancellationToken);

        return payments.Select(PaymentDto.From).ToList();
    }
}