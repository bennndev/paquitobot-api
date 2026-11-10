using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public record CreateUserStudentUseCaseCommand(
    Guid UserId,
    Guid StudentId,
    bool IsPrimary = true) : IRequest<UserStudentDto>;

public class CreateUserStudentUseCaseCommandValidator : AbstractValidator<CreateUserStudentUseCaseCommand>
{
    public CreateUserStudentUseCaseCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
    }
}

public class CreateUserStudentUseCaseCommandHandler (IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateUserStudentUseCaseCommand, UserStudentDto>
{

    public async Task<UserStudentDto> Handle(
        CreateUserStudentUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Repository<User>().ExistsAsync(request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        if (!await _unitOfWork.Repository<Student>().ExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

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
