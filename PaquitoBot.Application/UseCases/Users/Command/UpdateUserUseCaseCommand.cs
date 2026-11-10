using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public record UpdateUserUseCaseCommand(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    bool IsActive) : IRequest<UserDto>;

public class UpdateUserUseCaseCommandValidator : AbstractValidator<UpdateUserUseCaseCommand>
{
    public UpdateUserUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Username).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(c => c.Role).NotEmpty().MaximumLength(40);
    }
}

public class UpdateUserUseCaseCommandHandler : IRequestHandler<UpdateUserUseCaseCommand, UserDto>
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserUseCaseCommandHandler(IGenericRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(UpdateUserUseCaseCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
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
