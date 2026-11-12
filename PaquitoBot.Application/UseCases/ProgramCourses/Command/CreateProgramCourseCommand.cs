using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.ProgramCourses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Command;

public record CreateProgramCourseCommand(
    Guid ProgramId,
    Guid CourseId,
    int? CycleNumber = null,
    bool IsRequired = true) : IRequest<ProgramCourseDto>;

public class CreateProgramCourseCommandValidator
    : AbstractValidator<CreateProgramCourseCommand>
{
    public CreateProgramCourseCommandValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.CycleNumber)
            .Must(x => !x.HasValue || x.Value > 0)
            .WithMessage("CycleNumber must be greater than zero when provided.");
    }
}

public class CreateProgramCourseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProgramCourseCommand, ProgramCourseDto>
{
    public async Task<ProgramCourseDto> Handle(
        CreateProgramCourseCommand request,
        CancellationToken cancellationToken)
    {
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

        var link = new ProgramCourse
        {
            ProgramId = request.ProgramId,
            CourseId = request.CourseId,
            CycleNumber = request.CycleNumber,
            IsRequired = request.IsRequired
        };

        await unitOfWork.Repository<ProgramCourse>()
            .AddAsync(link, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramCourseDto.From(link);
    }
}
