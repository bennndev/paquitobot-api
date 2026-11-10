using MediatR;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Query;

public record GetAllUserStudentsUseCaseQuery : IRequest<IReadOnlyList<UserStudentDto>>;

public class GetAllUserStudentsUseCaseQueryHandler
    : IRequestHandler<GetAllUserStudentsUseCaseQuery, IReadOnlyList<UserStudentDto>>
{
    private readonly IUserStudentRepository _links;

    public GetAllUserStudentsUseCaseQueryHandler(IUserStudentRepository links)
    {
        _links = links;
    }

    public async Task<IReadOnlyList<UserStudentDto>> Handle(
        GetAllUserStudentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var links = await _links.GetAllAsync(cancellationToken);

        return links.Select(UserStudentDto.From).ToList();
    }
}
