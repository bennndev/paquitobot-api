using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Command;

public record DeleteAttendanceRecordCommand(Guid AttendanceId) : IRequest;

internal sealed class DeleteAttendanceRecordCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAttendanceRecordCommand>
{
    public async Task Handle(DeleteAttendanceRecordCommand request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Repository<AttendanceRecord>().GetByIdAsync(request.AttendanceId, cancellationToken)
            ?? throw new NotFoundException("AttendanceRecord", request.AttendanceId);

        unitOfWork.Repository<AttendanceRecord>().Remove(attendance);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
