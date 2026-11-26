using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Commands;

public record DeleteInstructorCommand(Guid InstructorId) : IRequest;

internal sealed class DeleteInstructorCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteInstructorCommand>
{
    public async Task Handle(DeleteInstructorCommand request, CancellationToken cancellationToken)
    {
        var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        _unitOfWork.Repository<Instructor>().Remove(instructor);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
