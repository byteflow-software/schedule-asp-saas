using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();

        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken)
            ?? throw new DomainException("INVALID_CREDENTIALS", "Invalid email or password.");

        if (!user.Tenant.IsActive)
            throw new DomainException("TENANT_INACTIVE", "This tenant account is inactive.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("INVALID_CREDENTIALS", "Invalid email or password.");

        var tokens = _jwtTokenService.GenerateTokens(user);

        var refreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedByIp = "system"
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            user.Id,
            user.TenantId,
            user.FullName,
            user.Role.ToString(),
            tokens);
    }
}
