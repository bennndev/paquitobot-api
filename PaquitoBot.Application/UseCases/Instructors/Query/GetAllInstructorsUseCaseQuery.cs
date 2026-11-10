using MediatR;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public record GetAllInstructorsUseCaseQuery : IRequest<IReadOnlyList<InstructorDto>>;

public class GetAllInstructorsUseCaseQueryHandler : IRequestHandler<GetAllInstructorsUseCaseQuery, IReadOnlyList<InstructorDto>>
{
    private readonly IGenericRepository<Instructor> _instructors;

    public GetAllInstructorsUseCaseQueryHandler(IGenericRepository<Instructor> instructors)
    {
        _instructors = instructors;
    }

    public async Task<IReadOnlyList<InstructorDto>> Handle(
        GetAllInstructorsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var instructors = await _instructors.GetAllAsync(cancellationToken);

        return instructors.Select(InstructorDto.From).ToList();
    }
}
