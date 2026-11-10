using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public record GetInstructorByIdUseCaseQuery(Guid InstructorId) : IRequest<InstructorDto>;

public class GetInstructorByIdUseCaseQueryHandler : IRequestHandler<GetInstructorByIdUseCaseQuery, InstructorDto>
{
    private readonly IGenericRepository<Instructor> _instructors;

    public GetInstructorByIdUseCaseQueryHandler(IGenericRepository<Instructor> instructors)
    {
        _instructors = instructors;
    }

    public async Task<InstructorDto> Handle(GetInstructorByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var instructor = await _instructors.GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        return InstructorDto.From(instructor);
    }
}
