using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Command;

public record DeleteAttendanceRecordUseCaseCommand(Guid AttendanceId) : IRequest;

internal sealed class DeleteAttendanceRecordUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAttendanceRecordUseCaseCommand>
{
    public async Task Handle(DeleteAttendanceRecordUseCaseCommand request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Repository<AttendanceRecord>().GetByIdAsync(request.AttendanceId, cancellationToken)
            ?? throw new NotFoundException("AttendanceRecord", request.AttendanceId);

        unitOfWork.Repository<AttendanceRecord>().Remove(attendance);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
