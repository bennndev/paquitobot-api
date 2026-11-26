using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Instructors;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Instructors.Commands;

public record UpdateInstructorCommand(
    Guid InstructorId,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    string? Status = null) : IRequest<InstructorDto>;

public class UpdateInstructorCommandValidator : AbstractValidator<UpdateInstructorCommand>
{
    public UpdateInstructorCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.FirstName).MaximumLength(160);
        RuleFor(c => c.LastName).MaximumLength(160);
        RuleFor(c => c.FullName).MaximumLength(320);
        RuleFor(c => c.Email).MaximumLength(254).EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email));
        RuleFor(c => c.Phone).MaximumLength(40);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class UpdateInstructorCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<UpdateInstructorCommand, InstructorDto>
{
    public async Task<InstructorDto> Handle(UpdateInstructorCommand request, CancellationToken cancellationToken)
    {
        var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId, cancellationToken)
            ?? throw new NotFoundException("Instructor", request.InstructorId);

        var firstName = TextNormalizer.Optional(request.FirstName);
        var lastName = TextNormalizer.Optional(request.LastName);
        var fullName = TextNormalizer.Optional(request.FullName);

        instructor.FirstName = firstName;
        instructor.LastName = lastName;
        instructor.FullName = ResolveFullName(fullName, firstName, lastName);
        instructor.Email = TextNormalizer.OptionalEmail(request.Email);
        instructor.Phone = TextNormalizer.Optional(request.Phone);
        instructor.Status = TextNormalizer.Optional(request.Status);
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
