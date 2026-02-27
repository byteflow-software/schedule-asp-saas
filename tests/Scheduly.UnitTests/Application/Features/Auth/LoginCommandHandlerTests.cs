using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Auth.Commands.Login;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.Identity;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Auth;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasherService _hasher;
    private readonly FakeJwtTokenService _jwtService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly Guid _tenantId = Guid.NewGuid();

    public LoginCommandHandlerTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        _hasher = new PasswordHasherService();
        _jwtService = new FakeJwtTokenService();
        _dateTimeProvider = new DateTimeProvider();
    }

    private void SeedUser(
        string email = "user@test.com",
        string password = "Password123!",
        bool isActive = true,
        bool tenantActive = true)
    {
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Test Clinic",
            Slug = "test-clinic",
            IsActive = tenantActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);

        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            FullName = "Test User",
            Email = email.ToLowerInvariant(),
            PasswordHash = _hasher.Hash(password),
            Role = UserRole.Admin,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    private LoginCommandHandler CreateHandler() =>
        new(_context, _hasher, _jwtService, _dateTimeProvider);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResult()
    {
        SeedUser("admin@clinic.com", "Secure123!");

        var result = await CreateHandler().Handle(
            new LoginCommand("admin@clinic.com", "Secure123!"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Role.Should().Be("Admin");
        result.TenantId.Should().Be(_tenantId);
        result.Tokens.AccessToken.Should().NotBeNullOrEmpty();
        result.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ValidCredentials_PersistsRefreshToken()
    {
        SeedUser("admin@clinic.com", "Secure123!");

        var result = await CreateHandler().Handle(
            new LoginCommand("admin@clinic.com", "Secure123!"),
            CancellationToken.None);

        var refreshToken = await _context.RefreshTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.UserId == result.UserId);

        refreshToken.Should().NotBeNull();
        refreshToken!.IsActive.Should().BeTrue();
        refreshToken.Token.Should().Be(result.Tokens.RefreshToken);
    }

    [Fact]
    public async Task Handle_EmailIsCaseInsensitive()
    {
        SeedUser("admin@clinic.com", "Secure123!");

        // Login with upper-cased email
        var result = await CreateHandler().Handle(
            new LoginCommand("ADMIN@CLINIC.COM", "Secure123!"),
            CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsDomainException()
    {
        SeedUser("admin@clinic.com", "CorrectPassword!");

        var act = () => CreateHandler().Handle(
            new LoginCommand("admin@clinic.com", "WrongPassword!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsDomainException()
    {
        var act = () => CreateHandler().Handle(
            new LoginCommand("ghost@clinic.com", "AnyPassword!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ThrowsDomainException()
    {
        SeedUser("inactive@clinic.com", "Password123!", isActive: false);

        var act = () => CreateHandler().Handle(
            new LoginCommand("inactive@clinic.com", "Password123!"),
            CancellationToken.None);

        // IsActive = false means the user is filtered out by IgnoreQueryFilters + IsActive check
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_InactiveTenant_ThrowsDomainException()
    {
        SeedUser("admin@closed.com", "Password123!", isActive: true, tenantActive: false);

        var act = () => CreateHandler().Handle(
            new LoginCommand("admin@closed.com", "Password123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "TENANT_INACTIVE");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
