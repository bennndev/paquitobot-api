using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Commands;

public record DeleteFeeStructureCommand(Guid FeeStructureId) : IRequest;

internal sealed class DeleteFeeStructureCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteFeeStructureCommand>
{
    public async Task Handle(DeleteFeeStructureCommand request, CancellationToken cancellationToken)
    {
        var feeStructure = await _unitOfWork.Repository<FeeStructure>()
                               .GetByIdAsync(request.FeeStructureId, cancellationToken)
                           ?? throw new NotFoundException("FeeStructure", request.FeeStructureId);

        _unitOfWork.Repository<FeeStructure>().Remove(feeStructure);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}