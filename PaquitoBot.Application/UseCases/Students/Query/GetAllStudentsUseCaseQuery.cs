using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public class GetAllStudentsUseCaseQuery
{
    private readonly IGenericRepository<Student> _students;

    public GetAllStudentsUseCaseQuery(IGenericRepository<Student> students)
    {
        _students = students;
    }

    public async Task<IReadOnlyList<StudentDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var students = await _students.GetAllAsync(cancellationToken);

        return students.Select(StudentDto.From).ToList();
    }
}
