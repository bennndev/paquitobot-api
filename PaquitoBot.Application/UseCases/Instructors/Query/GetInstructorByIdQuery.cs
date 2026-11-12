using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public record GetInstructorByIdQuery(Guid InstructorId) : IRequest<InstructorDto>;

public class GetInstructorByIdQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetInstructorByIdQuery, InstructorDto>
{
    public async Task<InstructorDto> Handle(GetInstructorByIdQuery request, CancellationToken cancellationToken)
    {
        var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        return InstructorDto.From(instructor);
    }
}
