using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Commands;

public record DeleteUserStudentCommand(Guid LinkId) : IRequest;

internal sealed class DeleteUserStudentCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserStudentCommand>
{
    public async Task Handle(DeleteUserStudentCommand request, CancellationToken cancellationToken)
    {
        var link = await _unitOfWork.UserStudents.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", request.LinkId);

        _unitOfWork.UserStudents.Remove(link);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
