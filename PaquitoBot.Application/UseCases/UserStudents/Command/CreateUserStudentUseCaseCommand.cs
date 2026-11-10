using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

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

public class CreateUserStudentUseCaseCommandHandler
    : IRequestHandler<CreateUserStudentUseCaseCommand, UserStudentDto>
{
    private readonly IUserStudentRepository _links;
    private readonly IGenericRepository<User> _users;
    private readonly IGenericRepository<Student> _students;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserStudentUseCaseCommandHandler(
        IUserStudentRepository links,
        IGenericRepository<User> users,
        IGenericRepository<Student> students,
        IUnitOfWork unitOfWork)
    {
        _links = links;
        _users = users;
        _students = students;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserStudentDto> Handle(
        CreateUserStudentUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _users.ExistsAsync(request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        if (!await _students.ExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        if (await _links.PairExistsAsync(request.UserId, request.StudentId, cancellationToken))
        {
            throw new ConflictException(
                $"User '{request.UserId}' is already linked to student '{request.StudentId}'.");
        }

        if (request.IsPrimary && await _links.UserHasPrimaryAsync(request.UserId, null, cancellationToken))
        {
            throw new ConflictException($"User '{request.UserId}' already has a primary student link.");
        }

        var link = new UserStudent
        {
            UserId = request.UserId,
            StudentId = request.StudentId,
            IsPrimary = request.IsPrimary
        };

        await _links.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserStudentDto.From(link);
    }
}
