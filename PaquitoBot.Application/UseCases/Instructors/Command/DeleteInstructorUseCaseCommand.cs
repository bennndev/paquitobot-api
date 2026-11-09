using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Command;

public class DeleteInstructorUseCaseCommand
{
    private readonly IGenericRepository<Instructor> _instructors;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInstructorUseCaseCommand(IGenericRepository<Instructor> instructors, IUnitOfWork unitOfWork)
    {
        _instructors = instructors;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var instructor = await _instructors.GetByIdAsync(instructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", instructorId);

        _instructors.Remove(instructor);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
