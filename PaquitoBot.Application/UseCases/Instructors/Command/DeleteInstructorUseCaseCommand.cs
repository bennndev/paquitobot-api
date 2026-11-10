using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Command;

public record DeleteInstructorUseCaseCommand(Guid InstructorId) : IRequest;

public class DeleteInstructorUseCaseCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteInstructorUseCaseCommand>
{
    public async Task Handle(DeleteInstructorUseCaseCommand request, CancellationToken cancellationToken)
    {
        var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        _unitOfWork.Repository<Instructor>().Remove(instructor);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
