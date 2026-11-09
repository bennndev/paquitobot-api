using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public class DeleteStudentUseCaseCommand
{
    private readonly IGenericRepository<Student> _students;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStudentUseCaseCommand(IGenericRepository<Student> students, IUnitOfWork unitOfWork)
    {
        _students = students;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("Student", studentId);

        _students.Remove(student);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
