using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.FeeStructures;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Command;

// Los datos que entran. La moneda por defecto es PEN, igual que en la base de datos
public record CreateFeeStructureUseCaseCommand(
    string Name,
    string? FrappeExternalId = null,
    Guid? ProgramId = null,
    string? Description = null,
    string CurrencyCode = "PEN",
    decimal? TotalAmount = null,
    string? Status = null) : IRequest<FeeStructureDto>;

// Las reglas: los largos máximos salen de las columnas de la base
public class CreateFeeStructureUseCaseCommandValidator : AbstractValidator<CreateFeeStructureUseCaseCommand>
{
    public CreateFeeStructureUseCaseCommandValidator()
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

// El manejador: arma la entidad, la guarda y devuelve el DTO
public class CreateFeeStructureUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateFeeStructureUseCaseCommand, FeeStructureDto>
{
    public async Task<FeeStructureDto> Handle(
        CreateFeeStructureUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        // Si mandaron un programa, debe existir
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