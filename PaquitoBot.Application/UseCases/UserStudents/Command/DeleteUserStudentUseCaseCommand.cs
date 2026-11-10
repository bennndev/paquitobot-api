using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public record DeleteUserStudentUseCaseCommand(Guid LinkId) : IRequest;

public class DeleteUserStudentUseCaseCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserStudentUseCaseCommand>
{
    public async Task Handle(DeleteUserStudentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var link = await _unitOfWork.UserStudents.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", request.LinkId);

        _unitOfWork.UserStudents.Remove(link);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
