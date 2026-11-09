using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Query;

public class GetUserStudentByIdUseCaseQuery
{
    private readonly IUserStudentRepository _links;

    public GetUserStudentByIdUseCaseQuery(IUserStudentRepository links)
    {
        _links = links;
    }

    public async Task<UserStudentDto> ExecuteAsync(Guid linkId, CancellationToken cancellationToken = default)
    {
        var link = await _links.GetByIdAsync(linkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", linkId);

        return UserStudentDto.From(link);
    }
}
