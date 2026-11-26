using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.FeeStructures;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Commands;

public record UpdateFeeStructureCommand(
    Guid FeeStructureId,
    string Name,
    string CurrencyCode,
    Guid? ProgramId = null,
    string? Description = null,
    decimal? TotalAmount = null,
    string? Status = null) : IRequest<FeeStructureDto>;

public class UpdateFeeStructureCommandValidator : AbstractValidator<UpdateFeeStructureCommand>
{
    public UpdateFeeStructureCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.TotalAmount).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false)
            .When(c => c.TotalAmount.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class UpdateFeeStructureCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateFeeStructureCommand, FeeStructureDto>
{
    public async Task<FeeStructureDto> Handle(
        UpdateFeeStructureCommand request,
        CancellationToken cancellationToken)
    {
        var feeStructure = await _unitOfWork.Repository<FeeStructure>()
            .GetByIdAsync(request.FeeStructureId, cancellationToken)
            ?? throw new NotFoundException("FeeStructure", request.FeeStructureId);

        if (request.ProgramId.HasValue
            && !await _unitOfWork.Repository<Program>().ExistsAsync(request.ProgramId.Value, cancellationToken))
        {
            throw new NotFoundException("Program", request.ProgramId.Value);
        }

        feeStructure.Name = TextNormalizer.Required(request.Name);
        feeStructure.ProgramId = request.ProgramId;
        feeStructure.Description = TextNormalizer.Optional(request.Description);
        feeStructure.CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant();
        feeStructure.TotalAmount = request.TotalAmount;
        feeStructure.Status = TextNormalizer.Optional(request.Status);
        feeStructure.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeeStructureDto.From(feeStructure);
    }
}