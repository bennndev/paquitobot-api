using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Query;

public class GetAllUserStudentsUseCaseQuery
{
    private readonly IUserStudentRepository _links;

    public GetAllUserStudentsUseCaseQuery(IUserStudentRepository links)
    {
        _links = links;
    }

    public async Task<IReadOnlyList<UserStudentDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var links = await _links.GetAllAsync(cancellationToken);

        return links.Select(UserStudentDto.From).ToList();
    }
}
