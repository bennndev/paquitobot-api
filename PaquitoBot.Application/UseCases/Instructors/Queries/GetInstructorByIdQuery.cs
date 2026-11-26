using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Instructors;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Queries;

public record GetInstructorByIdQuery(Guid InstructorId) : IRequest<InstructorDto>;

internal sealed class GetInstructorByIdQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetInstructorByIdQuery, InstructorDto>
{
    public async Task<InstructorDto> Handle(GetInstructorByIdQuery request, CancellationToken cancellationToken)
    {
        var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        return InstructorDto.From(instructor);
    }
}
