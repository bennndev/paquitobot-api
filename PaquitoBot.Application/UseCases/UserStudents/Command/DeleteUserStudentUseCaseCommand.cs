using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public record DeleteUserStudentUseCaseCommand(Guid LinkId) : IRequest;

public class DeleteUserStudentUseCaseCommandHandler : IRequestHandler<DeleteUserStudentUseCaseCommand>
{
    private readonly IUserStudentRepository _links;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserStudentUseCaseCommandHandler(IUserStudentRepository links, IUnitOfWork unitOfWork)
    {
        _links = links;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUserStudentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var link = await _links.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", request.LinkId);

        _links.Remove(link);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
