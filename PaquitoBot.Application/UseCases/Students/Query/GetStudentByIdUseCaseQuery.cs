using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public record GetStudentByIdUseCaseQuery(Guid StudentId) : IRequest<StudentDto>;

public class GetStudentByIdUseCaseQueryHandler : IRequestHandler<GetStudentByIdUseCaseQuery, StudentDto>
{
    private readonly IGenericRepository<Student> _students;

    public GetStudentByIdUseCaseQueryHandler(IGenericRepository<Student> students)
    {
        _students = students;
    }

    public async Task<StudentDto> Handle(GetStudentByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var student = await _students.GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        return StudentDto.From(student);
    }
}
