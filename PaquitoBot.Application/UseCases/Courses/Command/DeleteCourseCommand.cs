using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Command;

public record DeleteCourseCommand(Guid CourseId) : IRequest;

public class DeleteCourseCommandValidator
    : AbstractValidator<DeleteCourseCommand>
{
    public DeleteCourseCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class DeleteCourseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCourseCommand>
{
    public async Task Handle(
        DeleteCourseCommand request,
        CancellationToken cancellationToken)
    {
        var course = await unitOfWork.Repository<Course>()
            .GetByIdAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException(
                "Course", request.CourseId);

        unitOfWork.Repository<Course>().Remove(course);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
