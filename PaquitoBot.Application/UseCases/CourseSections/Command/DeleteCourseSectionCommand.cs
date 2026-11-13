using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Command;

public record DeleteCourseSectionCommand(Guid SectionId) : IRequest;

public class DeleteCourseSectionCommandValidator
    : AbstractValidator<DeleteCourseSectionCommand>
{
    public DeleteCourseSectionCommandValidator()
    {
        RuleFor(x => x.SectionId).NotEmpty();
    }
}

public class DeleteCourseSectionCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCourseSectionCommand>
{
    public async Task Handle(
        DeleteCourseSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await unitOfWork.Repository<CourseSection>()
            .GetByIdAsync(request.SectionId, cancellationToken)
            ?? throw new NotFoundException("CourseSection", request.SectionId);

        unitOfWork.Repository<CourseSection>().Remove(section);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
