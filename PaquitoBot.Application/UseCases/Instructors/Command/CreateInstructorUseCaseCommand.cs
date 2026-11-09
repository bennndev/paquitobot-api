using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Command;

public class CreateInstructorUseCaseCommand
{
    private readonly IGenericRepository<Instructor> _instructors;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstructorUseCaseCommand(IGenericRepository<Instructor> instructors, IUnitOfWork unitOfWork)
    {
        _instructors = instructors;
        _unitOfWork = unitOfWork;
    }

    public async Task<InstructorDto> ExecuteAsync(
        CreateInstructorRequest request,
        CancellationToken cancellationToken = default)
    {
        var firstName = RequestValidator.OptionalText(request.FirstName, "FirstName", 160);
        var lastName = RequestValidator.OptionalText(request.LastName, "LastName", 160);
        var fullName = RequestValidator.OptionalText(request.FullName, "FullName", 320);

        var instructor = new Instructor
        {
            FrappeExternalId = RequestValidator.OptionalText(request.FrappeExternalId, "FrappeExternalId", 140),
            FirstName = firstName,
            LastName = lastName,
            FullName = ResolveFullName(fullName, firstName, lastName),
            Email = RequestValidator.OptionalEmail(request.Email, "Email"),
            Phone = RequestValidator.OptionalText(request.Phone, "Phone", 40),
            Status = RequestValidator.OptionalText(request.Status, "Status", 40)
        };

        await _instructors.AddAsync(instructor, cancellationToken);
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
