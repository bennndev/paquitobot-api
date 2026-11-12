using MediatR;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public record GetAllStudentsQuery : IRequest<IReadOnlyList<StudentDto>>;

public class GetAllStudentsQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllStudentsQuery, IReadOnlyList<StudentDto>>
{
    public async Task<IReadOnlyList<StudentDto>> Handle(
        GetAllStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var students = await _unitOfWork.Repository<Student>().GetAllAsync(cancellationToken);

        return students.Select(StudentDto.From).ToList();
    }
}
