using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public record GetInstructorByIdUseCaseQuery(Guid InstructorId) : IRequest<InstructorDto>;

public class GetInstructorByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetInstructorByIdUseCaseQuery, InstructorDto>
{
    public async Task<InstructorDto> Handle(GetInstructorByIdUseCaseQuery request, CancellationToken cancellationToken)
    {
        var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        return InstructorDto.From(instructor);
    }
}
