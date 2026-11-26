using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Users;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Commands;

public record UpdateUserCommand(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    bool IsActive) : IRequest<UserDto>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Username).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(c => c.Role).NotEmpty().MaximumLength(40);
    }
}

internal sealed class UpdateUserCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Username = TextNormalizer.Required(request.Username);
        user.Email = TextNormalizer.RequiredEmail(request.Email);
        user.Role = TextNormalizer.Required(request.Role);
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
