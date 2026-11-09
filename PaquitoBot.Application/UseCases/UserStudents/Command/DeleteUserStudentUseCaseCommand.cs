using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public class DeleteUserStudentUseCaseCommand
{
    private readonly IUserStudentRepository _links;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserStudentUseCaseCommand(IUserStudentRepository links, IUnitOfWork unitOfWork)
    {
        _links = links;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid linkId, CancellationToken cancellationToken = default)
    {
        var link = await _links.GetByIdAsync(linkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", linkId);

        _links.Remove(link);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
