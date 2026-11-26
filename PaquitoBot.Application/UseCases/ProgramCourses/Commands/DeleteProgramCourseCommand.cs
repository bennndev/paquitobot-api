using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Commands;

public record DeleteProgramCourseCommand(Guid ProgramCourseId) : IRequest;

public class DeleteProgramCourseCommandValidator
    : AbstractValidator<DeleteProgramCourseCommand>
{
    public DeleteProgramCourseCommandValidator()
    {
        RuleFor(x => x.ProgramCourseId).NotEmpty();
    }
}

internal sealed class DeleteProgramCourseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProgramCourseCommand>
{
    public async Task Handle(
        DeleteProgramCourseCommand request,
        CancellationToken cancellationToken)
    {
        var link = await unitOfWork.Repository<ProgramCourse>()
            .GetByIdAsync(request.ProgramCourseId, cancellationToken)
            ?? throw new NotFoundException("ProgramCourse", request.ProgramCourseId);

        unitOfWork.Repository<ProgramCourse>().Remove(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
