using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Command;

public record CreateAttendanceRecordUseCaseCommand(
    Guid SectionId,
    Guid StudentId,
    DateOnly AttendanceDate,
    string Status,
    string? FrappeExternalId = null,
    string? Notes = null
) : IRequest<Guid>;

public class CreateAttendanceRecordUseCaseCommandValidator : AbstractValidator<CreateAttendanceRecordUseCaseCommand>
{
    public CreateAttendanceRecordUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.SectionId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.AttendanceDate).NotEmpty();
        RuleFor(c => c.Status).NotEmpty().MaximumLength(40);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
    }
}

internal sealed class CreateAttendanceRecordUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAttendanceRecordUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(CreateAttendanceRecordUseCaseCommand request, CancellationToken cancellationToken)
    {
        var attendance = new AttendanceRecord
        {
            SectionId = request.SectionId,
            StudentId = request.StudentId,
            AttendanceDate = request.AttendanceDate,
            Status = TextNormalizer.Required(request.Status),
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            Notes = TextNormalizer.Optional(request.Notes)
        };

        await unitOfWork.Repository<AttendanceRecord>().AddAsync(attendance, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return attendance.AttendanceId;
    }
}
