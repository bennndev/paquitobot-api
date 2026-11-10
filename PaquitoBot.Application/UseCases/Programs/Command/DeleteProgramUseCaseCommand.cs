
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Command;

public record DeleteProgramUseCaseCommand(Guid ProgramId) : IRequest;

public class DeleteProgramUseCaseCommandValidator
    : AbstractValidator<DeleteProgramUseCaseCommand>
{
    public DeleteProgramUseCaseCommandValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
    }
}

public class DeleteProgramUseCaseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProgramUseCaseCommand>
{
    public async Task Handle(
        DeleteProgramUseCaseCommand request,
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

