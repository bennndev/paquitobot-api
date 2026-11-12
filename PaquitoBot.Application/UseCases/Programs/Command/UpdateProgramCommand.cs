
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Programs;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Command;

public record UpdateProgramCommand(
    Guid ProgramId,
    string Name,
    string? Code = null,
    string? Description = null,
    string? Status = null) : IRequest<ProgramDto>;

public class UpdateProgramCommandValidator
    : AbstractValidator<UpdateProgramCommand>
{
    public UpdateProgramCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProgramId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(255);

        RuleFor(x => x.Code).MaximumLength(80);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class UpdateProgramCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProgramCommand, ProgramDto>
{
    public async Task<ProgramDto> Handle(
        UpdateProgramCommand request,
        CancellationToken cancellationToken)
    {
        var program = await unitOfWork.Repository<Program>()
            .GetByIdAsync(request.ProgramId, cancellationToken)
            ?? throw new NotFoundException(
                "Program", request.ProgramId);

        program.Name = TextNormalizer.Required(request.Name);
        program.Code = TextNormalizer.Optional(request.Code);
        program.Description = TextNormalizer.Optional(request.Description);
        program.Status = TextNormalizer.Optional(request.Status);
        program.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramDto.From(program);
    }
}
