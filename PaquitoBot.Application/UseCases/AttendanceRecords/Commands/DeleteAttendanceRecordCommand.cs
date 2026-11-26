using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Commands;

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
