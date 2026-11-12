using MediatR;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Query;

public record GetAllUserStudentsQuery : IRequest<IReadOnlyList<UserStudentDto>>;

public class GetAllUserStudentsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllUserStudentsQuery, IReadOnlyList<UserStudentDto>>
{


    public async Task<IReadOnlyList<UserStudentDto>> Handle(
        GetAllUserStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var links = await _unitOfWork.UserStudents.GetAllAsync(cancellationToken);

        return links.Select(UserStudentDto.From).ToList();
    }
}
