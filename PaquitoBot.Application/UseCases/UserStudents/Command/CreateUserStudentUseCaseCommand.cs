using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.UserStudents.Command;

public class CreateUserStudentUseCaseCommand
{
    private readonly IUserStudentRepository _links;
    private readonly IGenericRepository<User> _users;
    private readonly IGenericRepository<Student> _students;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserStudentUseCaseCommand(
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

    public async Task<UserStudentDto> ExecuteAsync(
        CreateUserStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequestValidator.RequireId(request.UserId, "UserId");
        var studentId = RequestValidator.RequireId(request.StudentId, "StudentId");

        if (!await _users.ExistsAsync(userId, cancellationToken))
        {
            throw new NotFoundException("User", userId);
        }

        if (!await _students.ExistsAsync(studentId, cancellationToken))
        {
            throw new NotFoundException("Student", studentId);
        }

        if (await _links.PairExistsAsync(userId, studentId, cancellationToken))
        {
            throw new ConflictException(
                $"User '{userId}' is already linked to student '{studentId}'.");
        }

        if (request.IsPrimary && await _links.UserHasPrimaryAsync(userId, null, cancellationToken))
        {
            throw new ConflictException($"User '{userId}' already has a primary student link.");
        }

        var link = new UserStudent
        {
            UserId = userId,
            StudentId = studentId,
            IsPrimary = request.IsPrimary
        };

        await _links.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserStudentDto.From(link);
    }
}
