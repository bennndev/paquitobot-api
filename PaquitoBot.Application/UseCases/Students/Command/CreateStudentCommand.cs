using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Command;

public record CreateStudentCommand(
    string FrappeExternalId,
    string? StudentCode = null,
    string? FirstNames = null,
    string? LastNames = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    DateOnly? BirthDate = null,
    string? Gender = null,
    string? Status = null) : IRequest<StudentDto>;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.FrappeExternalId).NotEmpty().MaximumLength(140);
        RuleFor(c => c.StudentCode).MaximumLength(80);
        RuleFor(c => c.FirstNames).MaximumLength(160);
        RuleFor(c => c.LastNames).MaximumLength(160);
        RuleFor(c => c.FullName).MaximumLength(320);
        RuleFor(c => c.Email).MaximumLength(254).EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email));
        RuleFor(c => c.Phone).MaximumLength(40);
        RuleFor(c => c.Gender).MaximumLength(40);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

public class CreateStudentCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<CreateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var firstNames = TextNormalizer.Optional(request.FirstNames);
        var lastNames = TextNormalizer.Optional(request.LastNames);
        var fullName = TextNormalizer.Optional(request.FullName) ?? BuildFullName(firstNames, lastNames);

        var student = new Student
        {
            FrappeExternalId = TextNormalizer.Required(request.FrappeExternalId),
            StudentCode = TextNormalizer.Optional(request.StudentCode),
            FirstNames = firstNames,
            LastNames = lastNames,
            FullName = fullName,
            Email = TextNormalizer.OptionalEmail(request.Email),
            Phone = TextNormalizer.Optional(request.Phone),
            BirthDate = request.BirthDate,
            Gender = TextNormalizer.Optional(request.Gender),
            Status = TextNormalizer.Optional(request.Status)
        };

        await _unitOfWork.Repository<Student>().AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StudentDto.From(student);
    }

    private static string? BuildFullName(string? firstNames, string? lastNames)
    {
        var fullName = $"{firstNames} {lastNames}".Trim();

        return fullName.Length == 0 ? null : fullName;
    }
}
