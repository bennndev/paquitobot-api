using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.UserStudents;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.UserStudents.Commands;

public record CreateUserStudentCommand(
    Guid UserId,
    Guid StudentId,
    bool IsPrimary = true) : IRequest<UserStudentDto>;

public class CreateUserStudentCommandValidator : AbstractValidator<CreateUserStudentCommand>
{
    public CreateUserStudentCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
    }
}

internal sealed class CreateUserStudentCommandHandler (IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateUserStudentCommand, UserStudentDto>
{

    public async Task<UserStudentDto> Handle(
        CreateUserStudentCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.Repository<User>().EnsureExistsAsync("User", request.UserId, cancellationToken);

        await _unitOfWork.Repository<Student>().EnsureExistsAsync("Student", request.StudentId, cancellationToken);

        if (await _unitOfWork.UserStudents.PairExistsAsync(request.UserId, request.StudentId, cancellationToken))
        {
            throw new ConflictException(
                $"User '{request.UserId}' is already linked to student '{request.StudentId}'.");
        }

        if (request.IsPrimary && await _unitOfWork.UserStudents.UserHasPrimaryAsync(request.UserId, null, cancellationToken))
        {
            throw new ConflictException($"User '{request.UserId}' already has a primary student link.");
        }

        var link = new UserStudent
        {
            UserId = request.UserId,
            StudentId = request.StudentId,
            IsPrimary = request.IsPrimary
        };

        await _unitOfWork.UserStudents.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserStudentDto.From(link);
    }
}
