using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public record UpdateUserStudentCommand(Guid LinkId, bool IsPrimary) : IRequest<UserStudentDto>;

public class UpdateUserStudentCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateUserStudentCommand, UserStudentDto>
{
    public async Task<UserStudentDto> Handle(UpdateUserStudentCommand request, CancellationToken cancellationToken)
    {
        var link = await _unitOfWork.UserStudents.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", request.LinkId);

        if (request.IsPrimary && await _unitOfWork.UserStudents.UserHasPrimaryAsync(link.UserId, link.LinkId, cancellationToken))
        {
            throw new ConflictException($"User '{link.UserId}' already has a primary student link.");
        }

        link.IsPrimary = request.IsPrimary;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserStudentDto.From(link);
    }
}
