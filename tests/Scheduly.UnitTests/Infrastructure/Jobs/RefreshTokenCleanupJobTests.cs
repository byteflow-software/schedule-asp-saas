using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.Jobs;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Jobs;

public class RefreshTokenCleanupJobTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenCleanupJob _sut;
    private readonly Guid _tenantId = Guid.NewGuid();
    private Guid _userId;

    public RefreshTokenCleanupJobTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);

        _sut = new RefreshTokenCleanupJob(
            _context, new DateTimeProvider(),
            NullLogger<RefreshTokenCleanupJob>.Instance);

        SeedTenant();
    }

    private void SeedTenant()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = _userId, TenantId = _tenantId, FullName = "Test",
            Email = "test@test.com", PasswordHash = "hash",
            Role = UserRole.Admin, IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task ExecuteAsync_RemovesExpiredTokens()
    {
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId, Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-10),
            CreatedAt = DateTime.UtcNow.AddDays(-17)
        });
        _context.SaveChanges();

        await _sut.ExecuteAsync();

        var remaining = await _context.RefreshTokens.IgnoreQueryFilters().CountAsync();
        remaining.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_RemovesRevokedTokens()
    {
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId, Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow.AddDays(-10),
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        });
        _context.SaveChanges();

        await _sut.ExecuteAsync();

        var remaining = await _context.RefreshTokens.IgnoreQueryFilters().CountAsync();
        remaining.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_KeepsActiveTokens()
    {
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId, Token = "active-token",
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        await _sut.ExecuteAsync();

        var remaining = await _context.RefreshTokens.IgnoreQueryFilters().CountAsync();
        remaining.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_NoTokens_CompletesWithoutError()
    {
        var act = () => _sut.ExecuteAsync();
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
