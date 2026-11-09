using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public class UpdateUserStudentUseCaseCommand
{
    private readonly IUserStudentRepository _links;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserStudentUseCaseCommand(IUserStudentRepository links, IUnitOfWork unitOfWork)
    {
        _links = links;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserStudentDto> ExecuteAsync(
        Guid linkId,
        UpdateUserStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await _links.GetByIdAsync(linkId, cancellationToken)
            ?? throw new NotFoundException("UserStudent", linkId);

        if (request.IsPrimary && await _links.UserHasPrimaryAsync(link.UserId, link.LinkId, cancellationToken))
        {
            throw new ConflictException($"User '{link.UserId}' already has a primary student link.");
        }

        link.IsPrimary = request.IsPrimary;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserStudentDto.From(link);
    }
}
