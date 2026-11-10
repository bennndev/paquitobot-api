using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Query;

public record GetStudentByIdUseCaseQuery(Guid StudentId) : IRequest<StudentDto>;

public class GetStudentByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetStudentByIdUseCaseQuery, StudentDto>
{
    public async Task<StudentDto> Handle(GetStudentByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        return StudentDto.From(student);
    }
}
