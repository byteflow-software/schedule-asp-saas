using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MustAsync(async (email, ct) =>
                !await context.Users.IgnoreQueryFilters()
                    .AnyAsync(u => u.Email == email.ToLowerInvariant(), ct))
            .WithMessage("Email is already registered.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
