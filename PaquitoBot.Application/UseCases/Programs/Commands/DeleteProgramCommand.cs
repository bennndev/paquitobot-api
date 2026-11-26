
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Commands;

public record DeleteProgramCommand(Guid ProgramId) : IRequest;

public class DeleteProgramCommandValidator
    : AbstractValidator<DeleteProgramCommand>
{
    public DeleteProgramCommandValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
    }
}

internal sealed class DeleteProgramCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProgramCommand>
{
    public async Task Handle(
        DeleteProgramCommand request,
        CancellationToken cancellationToken)
    {
        var program = await unitOfWork.Repository<Program>()
            .GetByIdAsync(request.ProgramId, cancellationToken)
            ?? throw new NotFoundException(
                "Program", request.ProgramId);

        unitOfWork.Repository<Program>().Remove(program);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}