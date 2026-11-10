using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Users.Command;

public record CreateUserUseCaseCommand(
    string Username,
    string Email,
    string Role,
    bool IsActive = true) : IRequest<UserDto>;

public class CreateUserUseCaseCommandValidator : AbstractValidator<CreateUserUseCaseCommand>
{
    public CreateUserUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Username).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(c => c.Role).NotEmpty().MaximumLength(40);
    }
}

public class CreateUserUseCaseCommandHandler : IRequestHandler<CreateUserUseCaseCommand, UserDto>
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserUseCaseCommandHandler(IGenericRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(CreateUserUseCaseCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = TextNormalizer.Required(request.Username),
            Email = TextNormalizer.RequiredEmail(request.Email),
            Role = TextNormalizer.Required(request.Role),
            IsActive = request.IsActive
        };

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
