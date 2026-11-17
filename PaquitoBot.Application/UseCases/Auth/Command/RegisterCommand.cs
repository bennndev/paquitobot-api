using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Auth.Command;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<Guid>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.Username).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8).MaximumLength(255);
    }
}

internal sealed class RegisterCommandHandler(IUnitOfWork unitOfWork, IUserPasswordHasher userPasswordHasher) : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = TextNormalizer.Required(request.Username),
            Email = TextNormalizer.RequiredEmail(request.Email),
            Role = "student",
            IsActive = true,
        };
        var passwordHash = userPasswordHasher.HashPassword(user, request.Password);

        user.PasswordHash = passwordHash;

        await unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.UserId;
    }
}
