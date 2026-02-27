using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Auth.Commands.RefreshToken;
using Scheduly.Application.Features.Auth.Commands.RevokeToken;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Auth;

public class RefreshAndRevokeTokenTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FakeJwtTokenService _jwtService;
    private readonly DateTimeProvider _dateTimeProvider = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public RefreshAndRevokeTokenTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        _jwtService = new FakeJwtTokenService();

        SeedData();
    }

    private void SeedData()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Clinic", Slug = "clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Users.Add(new User
        {
            Id = _userId, TenantId = _tenantId, FullName = "Admin",
            Email = "admin@clinic.com", PasswordHash = "h",
            Role = UserRole.Admin, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    private string SeedActiveRefreshToken()
    {
        var tokenValue = "active-token-" + Guid.NewGuid();
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "127.0.0.1"
        });
        _context.SaveChanges();
        return tokenValue;
    }

    private string SeedExpiredRefreshToken()
    {
        var tokenValue = "expired-token-" + Guid.NewGuid();
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Already expired
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            CreatedByIp = "127.0.0.1"
        });
        _context.SaveChanges();
        return tokenValue;
    }

    private string SeedRevokedRefreshToken()
    {
        var tokenValue = "revoked-token-" + Guid.NewGuid();
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "127.0.0.1",
            RevokedAt = DateTime.UtcNow.AddMinutes(-5) // Revoked
        });
        _context.SaveChanges();
        return tokenValue;
    }

    // ── RefreshToken ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokens()
    {
        var token = SeedActiveRefreshToken();
        var handler = new RefreshTokenCommandHandler(_context, _jwtService, _dateTimeProvider);

        var result = await handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(token, because: "A new refresh token must be issued");
    }

    [Fact]
    public async Task RefreshToken_ValidToken_RevokesOldToken()
    {
        var token = SeedActiveRefreshToken();
        var handler = new RefreshTokenCommandHandler(_context, _jwtService, _dateTimeProvider);

        await handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        var old = await _context.RefreshTokens.IgnoreQueryFilters()
            .FirstAsync(rt => rt.Token == token);
        old.IsRevoked.Should().BeTrue(because: "The consumed token must be revoked");
    }

    [Fact]
    public async Task RefreshToken_ValidToken_PersistsNewToken()
    {
        var token = SeedActiveRefreshToken();
        var handler = new RefreshTokenCommandHandler(_context, _jwtService, _dateTimeProvider);

        var result = await handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        var newToken = await _context.RefreshTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);
        newToken.Should().NotBeNull();
        newToken!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ThrowsDomainException()
    {
        var handler = new RefreshTokenCommandHandler(_context, _jwtService, _dateTimeProvider);

        var act = () => handler.Handle(new RefreshTokenCommand("does-not-exist"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_TOKEN");
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ThrowsDomainException()
    {
        var token = SeedExpiredRefreshToken();
        var handler = new RefreshTokenCommandHandler(_context, _jwtService, _dateTimeProvider);

        var act = () => handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_TOKEN");
    }

    [Fact]
    public async Task RefreshToken_RevokedToken_ThrowsDomainException()
    {
        var token = SeedRevokedRefreshToken();
        var handler = new RefreshTokenCommandHandler(_context, _jwtService, _dateTimeProvider);

        var act = () => handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_TOKEN");
    }

    // ── RevokeToken ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeToken_ActiveToken_SetsRevokedAt()
    {
        var token = SeedActiveRefreshToken();
        var handler = new RevokeTokenCommandHandler(_context, _dateTimeProvider);

        await handler.Handle(new RevokeTokenCommand(token), CancellationToken.None);

        var revoked = await _context.RefreshTokens.IgnoreQueryFilters()
            .FirstAsync(rt => rt.Token == token);
        revoked.RevokedAt.Should().NotBeNull();
        revoked.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeToken_AlreadyRevoked_ThrowsDomainException()
    {
        var token = SeedRevokedRefreshToken();
        var handler = new RevokeTokenCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new RevokeTokenCommand(token), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_TOKEN");
    }

    [Fact]
    public async Task RevokeToken_NonExistentToken_ThrowsDomainException()
    {
        var handler = new RevokeTokenCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new RevokeTokenCommand("ghost-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_TOKEN");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
