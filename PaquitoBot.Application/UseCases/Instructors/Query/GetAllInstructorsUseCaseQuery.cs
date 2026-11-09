using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public class GetAllInstructorsUseCaseQuery
{
    private readonly IGenericRepository<Instructor> _instructors;

    public GetAllInstructorsUseCaseQuery(IGenericRepository<Instructor> instructors)
    {
        _instructors = instructors;
    }

    public async Task<IReadOnlyList<InstructorDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var instructors = await _instructors.GetAllAsync(cancellationToken);

        return instructors.Select(InstructorDto.From).ToList();
    }
}
