using MediatR;
using PaquitoBot.Application.Dtos.Instructors;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Queries;

public record GetAllInstructorsQuery : IRequest<IReadOnlyList<InstructorDto>>;

internal sealed class GetAllInstructorsQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetAllInstructorsQuery, IReadOnlyList<InstructorDto>>
{
    public async Task<IReadOnlyList<InstructorDto>> Handle(
        GetAllInstructorsQuery request,
        CancellationToken cancellationToken)
    {
        var instructors = await _unitOfWork.Repository<Instructor>().GetAllAsync(cancellationToken);

        return instructors.Select(InstructorDto.From).ToList();
    }
}
