using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Students;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Students.Commands;

public record UpdateStudentCommand(
    Guid StudentId,
    string? StudentCode = null,
    string? FirstNames = null,
    string? LastNames = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    DateOnly? BirthDate = null,
    string? Gender = null,
    string? Status = null) : IRequest<StudentDto>;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

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

internal sealed class UpdateStudentCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<UpdateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var firstNames = TextNormalizer.Optional(request.FirstNames);
        var lastNames = TextNormalizer.Optional(request.LastNames);
        var fullName = TextNormalizer.Optional(request.FullName) ?? BuildFullName(firstNames, lastNames);

        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        student.StudentCode = TextNormalizer.Optional(request.StudentCode);
        student.FirstNames = firstNames;
        student.LastNames = lastNames;
        student.FullName = fullName;
        student.Email = TextNormalizer.OptionalEmail(request.Email);
        student.Phone = TextNormalizer.Optional(request.Phone);
        student.BirthDate = request.BirthDate;
        student.Gender = TextNormalizer.Optional(request.Gender);
        student.Status = TextNormalizer.Optional(request.Status);
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
