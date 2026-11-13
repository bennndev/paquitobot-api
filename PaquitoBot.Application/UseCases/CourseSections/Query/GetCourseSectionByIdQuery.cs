using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.CourseSections;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Query;

public record GetCourseSectionByIdQuery(Guid SectionId) : IRequest<CourseSectionDto>;

public class GetCourseSectionByIdQueryHandler(IUnitOfWork unitOfWork)
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
