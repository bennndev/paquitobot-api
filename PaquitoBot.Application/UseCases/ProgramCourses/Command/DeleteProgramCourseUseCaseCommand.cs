using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Command;

public record DeleteProgramCourseUseCaseCommand(Guid ProgramCourseId) : IRequest;

public class DeleteProgramCourseUseCaseCommandValidator
    : AbstractValidator<DeleteProgramCourseUseCaseCommand>
{
    public DeleteProgramCourseUseCaseCommandValidator()
    {
        RuleFor(x => x.ProgramCourseId).NotEmpty();
    }
}

public class DeleteProgramCourseUseCaseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProgramCourseUseCaseCommand>
{
    public async Task Handle(
        DeleteProgramCourseUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var link = await unitOfWork.Repository<ProgramCourse>()
            .GetByIdAsync(request.ProgramCourseId, cancellationToken)
            ?? throw new NotFoundException("ProgramCourse", request.ProgramCourseId);

        unitOfWork.Repository<ProgramCourse>().Remove(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
