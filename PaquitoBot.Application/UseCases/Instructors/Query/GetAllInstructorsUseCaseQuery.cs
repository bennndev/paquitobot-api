using MediatR;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Query;

public record GetAllInstructorsUseCaseQuery : IRequest<IReadOnlyList<InstructorDto>>;

public class GetAllInstructorsUseCaseQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllInstructorsUseCaseQuery, IReadOnlyList<InstructorDto>>
{
    public async Task<IReadOnlyList<InstructorDto>> Handle(
        GetAllInstructorsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var instructors = await _unitOfWork.Repository<Instructor>().GetAllAsync(cancellationToken);

        return instructors.Select(InstructorDto.From).ToList();
    }
}
