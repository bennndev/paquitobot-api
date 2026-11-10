using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Query;

public record GetUserStudentByIdUseCaseQuery(Guid LinkId) : IRequest<UserStudentDto>;

public class GetUserStudentByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetUserStudentByIdUseCaseQuery, UserStudentDto>
{
    public async Task<UserStudentDto> Handle(
        GetUserStudentByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var link = await _unitOfWork.UserStudents.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", request.LinkId);

        return UserStudentDto.From(link);
    }
}
