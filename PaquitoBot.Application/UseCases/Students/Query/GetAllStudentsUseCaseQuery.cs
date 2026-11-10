using MediatR;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public record GetAllStudentsUseCaseQuery : IRequest<IReadOnlyList<StudentDto>>;

public class GetAllStudentsUseCaseQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllStudentsUseCaseQuery, IReadOnlyList<StudentDto>>
{
    public async Task<IReadOnlyList<StudentDto>> Handle(
        GetAllStudentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var students = await _unitOfWork.Repository<Student>().GetAllAsync(cancellationToken);

        return students.Select(StudentDto.From).ToList();
    }
}
