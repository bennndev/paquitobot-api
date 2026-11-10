
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Programs;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Command;

public record CreateProgramUseCaseCommand(
    string FrappeExternalId,
    string Name,
    string? Code = null,
    string? Description = null,
    string? Status = null) : IRequest<ProgramDto>;

public class CreateProgramUseCaseCommandValidator
    : AbstractValidator<CreateProgramUseCaseCommand>
{
    public CreateProgramUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FrappeExternalId)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(140);

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(255);

        RuleFor(x => x.Code).MaximumLength(80);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class CreateProgramUseCaseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProgramUseCaseCommand, ProgramDto>
{
    public async Task<ProgramDto> Handle(
        CreateProgramUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var program = new Program
        {
            FrappeExternalId = TextNormalizer.Required(
                request.FrappeExternalId),
            Name = TextNormalizer.Required(request.Name),
            Code = TextNormalizer.Optional(request.Code),
            Description = TextNormalizer.Optional(request.Description),
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<Program>()
            .AddAsync(program, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramDto.From(program);
    }
}
