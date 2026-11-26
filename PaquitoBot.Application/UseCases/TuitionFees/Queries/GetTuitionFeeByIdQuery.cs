using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.TuitionFees;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Queries;

public record GetTuitionFeeByIdQuery(Guid TuitionFeeId) : IRequest<TuitionFeeDto>;

internal sealed class GetTuitionFeeByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetTuitionFeeByIdQuery, TuitionFeeDto>
{
    public async Task<TuitionFeeDto> Handle(
        GetTuitionFeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tuitionFee = await _unitOfWork.Repository<TuitionFee>()
                             .GetByIdAsync(request.TuitionFeeId, cancellationToken)
                         ?? throw new NotFoundException("TuitionFee", request.TuitionFeeId);

        return TuitionFeeDto.From(tuitionFee);
    }
}