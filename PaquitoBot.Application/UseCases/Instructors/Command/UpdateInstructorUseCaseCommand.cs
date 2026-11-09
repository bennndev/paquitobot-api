using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Command;

public class UpdateInstructorUseCaseCommand
{
    private readonly IGenericRepository<Instructor> _instructors;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInstructorUseCaseCommand(IGenericRepository<Instructor> instructors, IUnitOfWork unitOfWork)
    {
        _instructors = instructors;
        _unitOfWork = unitOfWork;
    }

    public async Task<InstructorDto> ExecuteAsync(
        Guid instructorId,
        UpdateInstructorRequest request,
        CancellationToken cancellationToken = default)
    {
        var firstName = RequestValidator.OptionalText(request.FirstName, "FirstName", 160);
        var lastName = RequestValidator.OptionalText(request.LastName, "LastName", 160);
        var fullName = RequestValidator.OptionalText(request.FullName, "FullName", 320);
        var email = RequestValidator.OptionalEmail(request.Email, "Email");
        var phone = RequestValidator.OptionalText(request.Phone, "Phone", 40);
        var status = RequestValidator.OptionalText(request.Status, "Status", 40);

        var instructor = await _instructors.GetByIdAsync(instructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", instructorId);

        instructor.FirstName = firstName;
        instructor.LastName = lastName;
        instructor.FullName = ResolveFullName(fullName, firstName, lastName);
        instructor.Email = email;
        instructor.Phone = phone;
        instructor.Status = status;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstructorDto.From(instructor);
    }

    private static string? ResolveFullName(string? fullName, string? firstName, string? lastName)
    {
        if (fullName is not null)
        {
            return fullName;
        }

        var computed = $"{firstName} {lastName}".Trim();

        return computed.Length == 0 ? null : computed;
    }
}
