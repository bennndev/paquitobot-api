using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.ProgramEnrollments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Commands;

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

internal sealed class CreateProgramEnrollmentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProgramEnrollmentCommand, ProgramEnrollmentDto>
{
    public async Task<ProgramEnrollmentDto> Handle(
        CreateProgramEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<Student>().EnsureExistsAsync("Student", request.StudentId, cancellationToken);

        await unitOfWork.Repository<Program>().EnsureExistsAsync("Program", request.ProgramId, cancellationToken);

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
