using System.Text.RegularExpressions;
using MediatR;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;

namespace Scheduly.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterCommandHandler(
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

    private static string GenerateSlug(string tenantName, Guid tenantId)
    {
        var base_ = Regex.Replace(tenantName.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        var suffix = tenantId.ToString("N")[..8];
        return $"{base_}-{suffix}";
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = request.TenantName,
            Slug = GenerateSlug(request.TenantName, tenantId),
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Tenants.Add(tenant);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Users.Add(user);

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

        return new RegisterResult(tenant.Id, user.Id, tokens);
    }
}
