using MediatR;
using PaquitoBot.Application.Dtos.Students;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Queries;

public record GetAllStudentsQuery : IRequest<IReadOnlyList<StudentDto>>;

internal sealed class GetAllStudentsQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllStudentsQuery, IReadOnlyList<StudentDto>>
{
    public async Task<IReadOnlyList<StudentDto>> Handle(
        GetAllStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var students = await _unitOfWork.Repository<Student>().GetAllAsync(cancellationToken);

        return students.Select(StudentDto.From).ToList();
    }
}
