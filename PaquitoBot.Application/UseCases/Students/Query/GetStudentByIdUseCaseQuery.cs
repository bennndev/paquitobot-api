using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public class GetStudentByIdUseCaseQuery
{
    private readonly IGenericRepository<Student> _students;

    public GetStudentByIdUseCaseQuery(IGenericRepository<Student> students)
    {
        _students = students;
    }

    public async Task<StudentDto> ExecuteAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("Student", studentId);

        return StudentDto.From(student);
    }
}
