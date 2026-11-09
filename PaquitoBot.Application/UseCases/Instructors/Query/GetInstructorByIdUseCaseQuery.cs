using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public class GetInstructorByIdUseCaseQuery
{
    private readonly IGenericRepository<Instructor> _instructors;

    public GetInstructorByIdUseCaseQuery(IGenericRepository<Instructor> instructors)
    {
        _instructors = instructors;
    }

    public async Task<InstructorDto> ExecuteAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var instructor = await _instructors.GetByIdAsync(instructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", instructorId);

        return InstructorDto.From(instructor);
    }
}
