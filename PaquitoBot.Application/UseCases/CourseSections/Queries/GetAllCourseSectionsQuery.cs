using MediatR;
using PaquitoBot.Application.Dtos.CourseSections;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Queries;

public record GetAllCourseSectionsQuery : IRequest<IReadOnlyList<CourseSectionDto>>;

internal sealed class GetAllCourseSectionsQueryHandler(IUnitOfWork unitOfWork)
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
