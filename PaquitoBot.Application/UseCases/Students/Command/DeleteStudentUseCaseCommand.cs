using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public record DeleteStudentUseCaseCommand(Guid StudentId) : IRequest;

public class DeleteStudentUseCaseCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteStudentUseCaseCommand>
{
    public async Task Handle(DeleteStudentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        _unitOfWork.Repository<Student>().Remove(student);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
