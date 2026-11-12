using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Command;

public record CreateProgramEnrollmentCommand(
    Guid StudentId,
    Guid ProgramId,
    Guid? AcademicPeriodId = null,
    string? FrappeExternalId = null,
    DateOnly? EnrollmentDate = null,
    string? Status = null) : IRequest<ProgramEnrollmentDto>;

public class CreateProgramEnrollmentCommandValidator
    : AbstractValidator<CreateProgramEnrollmentCommand>
{
    public CreateProgramEnrollmentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.AcademicPeriodId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("AcademicPeriodId must be a non-empty GUID when provided.");
        RuleFor(x => x.FrappeExternalId).MaximumLength(140);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class CreateProgramEnrollmentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProgramEnrollmentCommand, ProgramEnrollmentDto>
{
    public async Task<ProgramEnrollmentDto> Handle(
        CreateProgramEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Student>()
            .ExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        if (!await unitOfWork.Repository<Program>()
            .ExistsAsync(request.ProgramId, cancellationToken))
        {
            throw new NotFoundException("Program", request.ProgramId);
        }

        if (request.AcademicPeriodId is Guid periodId &&
            !await unitOfWork.Repository<AcademicPeriod>()
                .ExistsAsync(periodId, cancellationToken))
        {
            throw new NotFoundException("AcademicPeriod", periodId);
        }

        var enrollment = new ProgramEnrollment
        {
            StudentId = request.StudentId,
            ProgramId = request.ProgramId,
            AcademicPeriodId = request.AcademicPeriodId,
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            EnrollmentDate = request.EnrollmentDate,
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<ProgramEnrollment>()
            .AddAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramEnrollmentDto.From(enrollment);
    }
}
