using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public record DeleteStudentUseCaseCommand(Guid StudentId) : IRequest;

public class DeleteStudentUseCaseCommandHandler : IRequestHandler<DeleteStudentUseCaseCommand>
{
    private readonly IGenericRepository<Student> _students;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStudentUseCaseCommandHandler(IGenericRepository<Student> students, IUnitOfWork unitOfWork)
    {
        _students = students;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteStudentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var student = await _students.GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        _students.Remove(student);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
