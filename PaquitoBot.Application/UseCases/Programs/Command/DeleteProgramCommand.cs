
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Command;

public record DeleteProgramCommand(Guid ProgramId) : IRequest;

public class DeleteProgramCommandValidator
    : AbstractValidator<DeleteProgramCommand>
{
    public DeleteProgramCommandValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
    }
}

public class DeleteProgramCommandHandler(
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