using MediatR;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Students.Queries;

public sealed record GetFrappeStudentsQuery : IRequest<IReadOnlyList<FrappeStudentDto>>;

internal sealed class GetFrappeStudentsQueryHandler(IFrappeSource<FrappeStudentDto> source)
    : IRequestHandler<GetFrappeStudentsQuery, IReadOnlyList<FrappeStudentDto>>
{
    public Task<IReadOnlyList<FrappeStudentDto>> Handle(GetFrappeStudentsQuery request, CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);
}