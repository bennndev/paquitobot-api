using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.UserStudents;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Queries;

public record GetUserStudentByIdQuery(Guid LinkId) : IRequest<UserStudentDto>;

internal sealed class GetUserStudentByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetUserStudentByIdQuery, UserStudentDto>
{
    public async Task<UserStudentDto> Handle(
        GetUserStudentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var link = await _unitOfWork.UserStudents.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", request.LinkId);

        return UserStudentDto.From(link);
    }
}
