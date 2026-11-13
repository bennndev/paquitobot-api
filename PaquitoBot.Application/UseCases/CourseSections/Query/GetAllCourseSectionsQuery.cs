using MediatR;
using PaquitoBot.Application.DTOs.CourseSections;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Query;

public record GetAllCourseSectionsQuery : IRequest<IReadOnlyList<CourseSectionDto>>;

public class GetAllCourseSectionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllCourseSectionsQuery, IReadOnlyList<CourseSectionDto>>
{
    public async Task<IReadOnlyList<CourseSectionDto>> Handle(
        GetAllCourseSectionsQuery request,
        CancellationToken cancellationToken)
    {
        var sections = await unitOfWork.Repository<CourseSection>()
            .GetAllAsync(cancellationToken);

        return sections.Select(CourseSectionDto.From).ToList();
    }
}
