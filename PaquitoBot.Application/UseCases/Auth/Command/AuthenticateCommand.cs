using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Auth.Command;

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
        //Validar contraseña
        if (entity is null || !entity.IsActive || string.IsNullOrEmpty(entity.PasswordHash) || !userPasswordHasher.ValidatePassword(
                entity,
                entity.PasswordHash,
                request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        //Genera y retorna Token si paso validacion
        var token = tokenService.GenerateToken(
            entity.UserId,
            entity.Role,
            entity.Email);

        return LoginResponse.From(entity.UserId, token, entity.Email, entity.Role);
    }
}
