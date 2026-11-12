using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public record DeleteStudentCommand(Guid StudentId) : IRequest;

public class DeleteStudentCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteStudentCommand>
{
    public async Task Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        _unitOfWork.Repository<Student>().Remove(student);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
