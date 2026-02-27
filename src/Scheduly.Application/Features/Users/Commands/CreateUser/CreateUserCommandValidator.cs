using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress()
            .MustAsync(async (email, ct) =>
                !await context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct))
            .WithMessage("Email is already in use within this tenant.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty().Must(r =>
            r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be 'Admin' or 'Staff'.");
    }
}
