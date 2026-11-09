using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Validation;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public class UpdateStudentUseCaseCommand
{
    private readonly IGenericRepository<Student> _students;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentUseCaseCommand(IGenericRepository<Student> students, IUnitOfWork unitOfWork)
    {
        _students = students;
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentDto> ExecuteAsync(
        Guid studentId,
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var studentCode = RequestValidator.OptionalText(request.StudentCode, "StudentCode", 80);
        var firstNames = RequestValidator.OptionalText(request.FirstNames, "FirstNames", 160);
        var lastNames = RequestValidator.OptionalText(request.LastNames, "LastNames", 160);
        var fullName = RequestValidator.OptionalText(request.FullName, "FullName", 320)
            ?? BuildFullName(firstNames, lastNames);
        var email = RequestValidator.OptionalEmail(request.Email, "Email");
        var phone = RequestValidator.OptionalText(request.Phone, "Phone", 40);
        var gender = RequestValidator.OptionalText(request.Gender, "Gender", 40);
        var status = RequestValidator.OptionalText(request.Status, "Status", 40);

        var student = await _students.GetByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("Student", studentId);

        student.StudentCode = studentCode;
        student.FirstNames = firstNames;
        student.LastNames = lastNames;
        student.FullName = fullName;
        student.Email = email;
        student.Phone = phone;
        student.BirthDate = request.BirthDate;
        student.Gender = gender;
        student.Status = status;
        student.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StudentDto.From(student);
    }

    private static string? BuildFullName(string? firstNames, string? lastNames)
    {
        var fullName = $"{firstNames} {lastNames}".Trim();

        return fullName.Length == 0 ? null : fullName;
    }
}
