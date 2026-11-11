using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.ProgramCourses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Command;

public record UpdateProgramCourseUseCaseCommand(
    Guid ProgramCourseId,
    Guid ProgramId,
    Guid CourseId,
    int? CycleNumber = null,
    bool IsRequired = true) : IRequest<ProgramCourseDto>;

public class UpdateProgramCourseUseCaseCommandValidator
    : AbstractValidator<UpdateProgramCourseUseCaseCommand>
{
    public UpdateProgramCourseUseCaseCommandValidator()
    {
        RuleFor(x => x.ProgramCourseId).NotEmpty();
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.CycleNumber)
            .Must(x => !x.HasValue || x.Value > 0)
            .WithMessage("CycleNumber must be greater than zero when provided.");
    }
}

public class UpdateProgramCourseUseCaseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProgramCourseUseCaseCommand, ProgramCourseDto>
{
    public async Task<ProgramCourseDto> Handle(
        UpdateProgramCourseUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var link = await unitOfWork.Repository<ProgramCourse>()
            .GetByIdAsync(request.ProgramCourseId, cancellationToken)
            ?? throw new NotFoundException("ProgramCourse", request.ProgramCourseId);

        if (!await unitOfWork.Repository<Program>()
            .ExistsAsync(request.ProgramId, cancellationToken))
        {
            throw new NotFoundException("Program", request.ProgramId);
        }

        if (!await unitOfWork.Repository<Course>()
            .ExistsAsync(request.CourseId, cancellationToken))
        {
            throw new NotFoundException("Course", request.CourseId);
        }

        link.ProgramId = request.ProgramId;
        link.CourseId = request.CourseId;
        link.CycleNumber = request.CycleNumber;
        link.IsRequired = request.IsRequired;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramCourseDto.From(link);
    }
}
