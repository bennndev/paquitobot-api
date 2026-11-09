using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public class CreateStudentUseCaseCommand
{
    private readonly IGenericRepository<Student> _students;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStudentUseCaseCommand(IGenericRepository<Student> students, IUnitOfWork unitOfWork)
    {
        _students = students;
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentDto> ExecuteAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var firstNames = RequestValidator.OptionalText(request.FirstNames, "FirstNames", 160);
        var lastNames = RequestValidator.OptionalText(request.LastNames, "LastNames", 160);
        var fullName = RequestValidator.OptionalText(request.FullName, "FullName", 320)
            ?? BuildFullName(firstNames, lastNames);

        var student = new Student
        {
            FrappeExternalId = RequestValidator.RequireText(request.FrappeExternalId, "FrappeExternalId", 140),
            StudentCode = RequestValidator.OptionalText(request.StudentCode, "StudentCode", 80),
            FirstNames = firstNames,
            LastNames = lastNames,
            FullName = fullName,
            Email = RequestValidator.OptionalEmail(request.Email, "Email"),
            Phone = RequestValidator.OptionalText(request.Phone, "Phone", 40),
            BirthDate = request.BirthDate,
            Gender = RequestValidator.OptionalText(request.Gender, "Gender", 40),
            Status = RequestValidator.OptionalText(request.Status, "Status", 40)
        };

        await _students.AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StudentDto.From(student);
    }

    private static string? BuildFullName(string? firstNames, string? lastNames)
    {
        var fullName = $"{firstNames} {lastNames}".Trim();

        return fullName.Length == 0 ? null : fullName;
    }
}
