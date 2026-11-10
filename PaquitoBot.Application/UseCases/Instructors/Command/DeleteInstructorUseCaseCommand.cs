using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Command;

public record DeleteInstructorUseCaseCommand(Guid InstructorId) : IRequest;

public class DeleteInstructorUseCaseCommandHandler : IRequestHandler<DeleteInstructorUseCaseCommand>
{
    private readonly IGenericRepository<Instructor> _instructors;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInstructorUseCaseCommandHandler(IGenericRepository<Instructor> instructors, IUnitOfWork unitOfWork)
    {
        _instructors = instructors;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteInstructorUseCaseCommand request, CancellationToken cancellationToken)
    {
        var instructor = await _instructors.GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        _instructors.Remove(instructor);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
