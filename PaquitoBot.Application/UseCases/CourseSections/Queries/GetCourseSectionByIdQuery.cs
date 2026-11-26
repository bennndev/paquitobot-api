using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.CourseSections;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Queries;

public record GetCourseSectionByIdQuery(Guid SectionId) : IRequest<CourseSectionDto>;

internal sealed class GetCourseSectionByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCourseSectionByIdQuery, CourseSectionDto>
{
    public async Task<CourseSectionDto> Handle(
        GetCourseSectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var section = await unitOfWork.Repository<CourseSection>()
            .GetByIdAsync(request.SectionId, cancellationToken)
            ?? throw new NotFoundException("CourseSection", request.SectionId);

        return CourseSectionDto.From(section);
    }
}
