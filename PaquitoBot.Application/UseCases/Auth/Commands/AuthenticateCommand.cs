using FluentValidation;
using MediatR;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Auth;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Auth.Commands;

public record AuthenticateCommand(
    string Email,
    string Password
): IRequest<LoginResponse>;

public class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.Email).NotEmpty();
        RuleFor(c => c.Password).NotEmpty();
    }
}

internal sealed class AuthenticateCommandHandler(IUnitOfWork _unitOfWork,
    IUserPasswordHasher userPasswordHasher,
    ITokenService tokenService) : IRequestHandler<AuthenticateCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Users.GetByEmailAsync(TextNormalizer.RequiredEmail(request.Email), cancellationToken);
        if (entity is null || !entity.IsActive || string.IsNullOrEmpty(entity.PasswordHash) || !userPasswordHasher.ValidatePassword(
                entity,
                entity.PasswordHash,
                request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var studentId = await _unitOfWork.UserStudents.GetPrimaryStudentIdAsync(entity.UserId, cancellationToken);
        var token = tokenService.GenerateToken(
            entity.UserId,
            entity.Role,
            entity.Email,
            studentId);

        return LoginResponse.From(entity.UserId, token, entity.Email, entity.Role);
    }
}
