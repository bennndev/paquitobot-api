using MediatR;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public record GetAllStudentsUseCaseQuery : IRequest<IReadOnlyList<StudentDto>>;

public class GetAllStudentsUseCaseQueryHandler : IRequestHandler<GetAllStudentsUseCaseQuery, IReadOnlyList<StudentDto>>
{
    private readonly IGenericRepository<Student> _students;

    public GetAllStudentsUseCaseQueryHandler(IGenericRepository<Student> students)
    {
        _students = students;
    }

    public async Task<IReadOnlyList<StudentDto>> Handle(
        GetAllStudentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var students = await _students.GetAllAsync(cancellationToken);

        return students.Select(StudentDto.From).ToList();
    }
}
