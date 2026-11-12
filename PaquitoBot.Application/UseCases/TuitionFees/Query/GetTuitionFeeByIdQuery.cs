using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.TuitionFees;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Query;

public record GetTuitionFeeByIdQuery(Guid TuitionFeeId) : IRequest<TuitionFeeDto>;

public class GetTuitionFeeByIdQueryHandler(IUnitOfWork _unitOfWork)
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