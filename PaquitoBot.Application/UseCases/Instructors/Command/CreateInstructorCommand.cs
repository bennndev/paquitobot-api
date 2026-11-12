using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Command;

public record CreateInstructorCommand(
    string? FrappeExternalId = null,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    string? Status = null) : IRequest<InstructorDto>;

public class CreateInstructorCommandValidator : AbstractValidator<CreateInstructorCommand>
{
    public CreateInstructorCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.FirstName).MaximumLength(160);
        RuleFor(c => c.LastName).MaximumLength(160);
        RuleFor(c => c.FullName).MaximumLength(320);
        RuleFor(c => c.Email).MaximumLength(254).EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email));
        RuleFor(c => c.Phone).MaximumLength(40);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

public class CreateInstructorCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<CreateInstructorCommand, InstructorDto>
{
    public async Task<InstructorDto> Handle(CreateInstructorCommand request, CancellationToken cancellationToken)
    {
        var firstName = TextNormalizer.Optional(request.FirstName);
        var lastName = TextNormalizer.Optional(request.LastName);
        var fullName = TextNormalizer.Optional(request.FullName);

        var instructor = new Instructor
        {
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            FirstName = firstName,
            LastName = lastName,
            FullName = ResolveFullName(fullName, firstName, lastName),
            Email = TextNormalizer.OptionalEmail(request.Email),
            Phone = TextNormalizer.Optional(request.Phone),
            Status = TextNormalizer.Optional(request.Status)
        };

        await _unitOfWork.Repository<Instructor>().AddAsync(instructor, cancellationToken);
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
