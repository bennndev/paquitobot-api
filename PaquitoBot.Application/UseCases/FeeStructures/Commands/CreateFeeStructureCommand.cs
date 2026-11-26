using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.FeeStructures;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Commands;

public record CreateFeeStructureCommand(
    string Name,
    string? FrappeExternalId = null,
    Guid? ProgramId = null,
    string? Description = null,
    string CurrencyCode = "PEN",
    decimal? TotalAmount = null,
    string? Status = null) : IRequest<FeeStructureDto>;

public class CreateFeeStructureCommandValidator : AbstractValidator<CreateFeeStructureCommand>
{
    public CreateFeeStructureCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.TotalAmount).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false)
            .When(c => c.TotalAmount.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class CreateFeeStructureCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateFeeStructureCommand, FeeStructureDto>
{
    public async Task<FeeStructureDto> Handle(
        CreateFeeStructureCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ProgramId.HasValue
            && !await _unitOfWork.Repository<Program>().ExistsAsync(request.ProgramId.Value, cancellationToken))
        {
            throw new NotFoundException("Program", request.ProgramId.Value);
        }

        var feeStructure = new FeeStructure
        {
            Name = TextNormalizer.Required(request.Name),
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            ProgramId = request.ProgramId,
            Description = TextNormalizer.Optional(request.Description),
            CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant(),
            TotalAmount = request.TotalAmount,
            Status = TextNormalizer.Optional(request.Status)
        };

        await _unitOfWork.Repository<FeeStructure>().AddAsync(feeStructure, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeeStructureDto.From(feeStructure);
    }
}