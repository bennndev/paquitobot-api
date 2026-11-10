using MediatR;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Query;

public record GetAllUserStudentsUseCaseQuery : IRequest<IReadOnlyList<UserStudentDto>>;

public class GetAllUserStudentsUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllUserStudentsUseCaseQuery, IReadOnlyList<UserStudentDto>>
{


    public async Task<IReadOnlyList<UserStudentDto>> Handle(
        GetAllUserStudentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var links = await _unitOfWork.UserStudents.GetAllAsync(cancellationToken);

        return links.Select(UserStudentDto.From).ToList();
    }
}
