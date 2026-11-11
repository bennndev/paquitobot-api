using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Command;

public record UpdateAttendanceRecordUseCaseCommand(
    Guid AttendanceId,
    Guid SectionId,
    Guid StudentId,
    DateOnly AttendanceDate,
    string Status,
    string? FrappeExternalId = null,
    string? Notes = null
) : IRequest<Guid>;

public class UpdateAttendanceRecordUseCaseCommandValidator : AbstractValidator<UpdateAttendanceRecordUseCaseCommand>
{
    public UpdateAttendanceRecordUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.AttendanceId).NotEmpty();
        RuleFor(c => c.SectionId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.AttendanceDate).NotEmpty();
        RuleFor(c => c.Status).NotEmpty().MaximumLength(40);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
    }
}

internal sealed class UpdateAttendanceRecordUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAttendanceRecordUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(UpdateAttendanceRecordUseCaseCommand request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Repository<AttendanceRecord>().GetByIdAsync(request.AttendanceId, cancellationToken)
            ?? throw new NotFoundException("AttendanceRecord", request.AttendanceId);

        attendance.SectionId = request.SectionId;
        attendance.StudentId = request.StudentId;
        attendance.AttendanceDate = request.AttendanceDate;
        attendance.Status = TextNormalizer.Required(request.Status);
        attendance.FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId);
        attendance.Notes = TextNormalizer.Optional(request.Notes);
        attendance.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return attendance.AttendanceId;
    }
}
