using MediatR;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public record GetAllInstructorsQuery : IRequest<IReadOnlyList<InstructorDto>>;

public class GetAllInstructorsQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllInstructorsQuery, IReadOnlyList<InstructorDto>>
{
    public async Task<IReadOnlyList<InstructorDto>> Handle(
        GetAllInstructorsQuery request,
        CancellationToken cancellationToken)
    {
        var instructors = await _unitOfWork.Repository<Instructor>().GetAllAsync(cancellationToken);

        return instructors.Select(InstructorDto.From).ToList();
    }
}
