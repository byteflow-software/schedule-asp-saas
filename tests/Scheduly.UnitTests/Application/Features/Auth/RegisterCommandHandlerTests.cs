using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Auth.Commands.Register;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.Identity;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Auth;

/// <summary>
/// Tests for RegisterCommandHandler covering slug generation,
/// token generation, and the critical email-uniqueness validator fix.
/// </summary>
public class RegisterCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly FakeJwtTokenService _jwtService;
    private readonly PasswordHasherService _hasher;
    private readonly DateTimeProvider _dateTimeProvider;

    public RegisterCommandHandlerTests()
    {
        _tenantService = new CurrentTenantService();
        // TenantId is Guid.Empty before registration — this is the real-world state
        // when the validator/handler runs for a new registration.

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);
        _jwtService = new FakeJwtTokenService();
        _hasher = new PasswordHasherService();
        _dateTimeProvider = new DateTimeProvider();
    }

    private RegisterCommandHandler CreateHandler() =>
        new(_context, _hasher, _jwtService, _dateTimeProvider);

    private RegisterCommandValidator CreateValidator() =>
        new(_context);

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidRequest_CreatesTenantAndUser()
    {
        var command = new RegisterCommand(
            TenantName: "Sunrise Dental",
            FullName: "Dr. Smith",
            Email: "smith@sunrise.com",
            Password: "Secret123!");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.TenantId.Should().NotBeEmpty();
        result.UserId.Should().NotBeEmpty();
        result.Tokens.AccessToken.Should().NotBeNullOrEmpty();
        result.Tokens.RefreshToken.Should().NotBeNullOrEmpty();

        var tenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == result.TenantId);
        tenant.Should().NotBeNull();
        tenant!.IsActive.Should().BeTrue();

        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == result.UserId);
        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.Admin);
        user.Email.Should().Be("smith@sunrise.com");

        var token = await _context.RefreshTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.UserId == result.UserId);
        token.Should().NotBeNull();
        token!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SlugIsUrlSafeAndUnique()
    {
        var command1 = new RegisterCommand("Test Clinic", "Admin", "a@test.com", "Pass1234!");
        var command2 = new RegisterCommand("Test Clinic", "Admin", "b@test.com", "Pass1234!");

        var r1 = await CreateHandler().Handle(command1, CancellationToken.None);
        var r2 = await CreateHandler().Handle(command2, CancellationToken.None);

        var tenant1 = await _context.Tenants.IgnoreQueryFilters().FirstAsync(t => t.Id == r1.TenantId);
        var tenant2 = await _context.Tenants.IgnoreQueryFilters().FirstAsync(t => t.Id == r2.TenantId);

        // Slugs must be different even for identical tenant names
        tenant1.Slug.Should().NotBe(tenant2.Slug);

        // Slugs must be URL-safe (only lowercase alphanumeric and dashes)
        tenant1.Slug.Should().MatchRegex(@"^[a-z0-9\-]+$");
        tenant2.Slug.Should().MatchRegex(@"^[a-z0-9\-]+$");
    }

    [Fact]
    public async Task Handle_SpecialCharactersInTenantName_ProducesSafeSlug()
    {
        var command = new RegisterCommand(
            "O'Brien & Associés!", "Admin", "admin@obrien.com", "Pass1234!");

        var result = await CreateHandler().Handle(command, CancellationToken.None);
        var tenant = await _context.Tenants.IgnoreQueryFilters().FirstAsync(t => t.Id == result.TenantId);

        tenant.Slug.Should().MatchRegex(@"^[a-z0-9\-]+$",
            because: "Slug must contain only URL-safe characters");
    }

    [Fact]
    public async Task Handle_EmailIsStoredLowercased()
    {
        var command = new RegisterCommand("Clinic", "Admin", "UPPER@TEST.COM", "Pass1234!");

        var result = await CreateHandler().Handle(command, CancellationToken.None);
        var user = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == result.UserId);

        user.Email.Should().Be("upper@test.com");
    }

    [Fact]
    public async Task Handle_PasswordIsHashed()
    {
        var plainPassword = "Password123!";
        var command = new RegisterCommand("Clinic", "Admin", "admin@clinic.com", plainPassword);

        var result = await CreateHandler().Handle(command, CancellationToken.None);
        var user = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == result.UserId);

        user.PasswordHash.Should().NotBe(plainPassword,
            because: "Password must be stored as a hash, not plaintext");

        var verifies = new PasswordHasherService().Verify(plainPassword, user.PasswordHash);
        verifies.Should().BeTrue();
    }

    // ── Email uniqueness (the critical bug fix) ─────────────────────────────

    [Fact]
    public async Task Validator_DuplicateEmailAcrossTenants_FailsValidation()
    {
        // Register first tenant with the email
        var firstCommand = new RegisterCommand("Clinic A", "Admin A", "shared@email.com", "Pass1234!");
        await CreateHandler().Handle(firstCommand, CancellationToken.None);

        // Validator must catch the duplicate email even though TenantId is still Guid.Empty
        // (the bug was that the query filter made AnyAsync() return false for all users)
        var validator = CreateValidator();
        var secondCommand = new RegisterCommand("Clinic B", "Admin B", "shared@email.com", "Pass1234!");

        var validationResult = await validator.ValidateAsync(secondCommand);

        validationResult.IsValid.Should().BeFalse(
            because: "Email 'shared@email.com' is already registered in another tenant");
        validationResult.Errors.Should().Contain(e =>
            e.PropertyName == "Email" && e.ErrorMessage.Contains("already registered"));
    }

    [Fact]
    public async Task Validator_UniqueEmail_PassesValidation()
    {
        var validator = CreateValidator();
        var command = new RegisterCommand("Clinic", "Admin", "unique@email.com", "Pass1234!");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_EmptyTenantName_FailsValidation()
    {
        var validator = CreateValidator();
        var result = await validator.ValidateAsync(
            new RegisterCommand("", "Admin", "a@b.com", "Pass1234!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantName");
    }

    [Fact]
    public async Task Validator_ShortPassword_FailsValidation()
    {
        var validator = CreateValidator();
        var result = await validator.ValidateAsync(
            new RegisterCommand("Clinic", "Admin", "a@b.com", "short"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validator_InvalidEmail_FailsValidation()
    {
        var validator = CreateValidator();
        var result = await validator.ValidateAsync(
            new RegisterCommand("Clinic", "Admin", "not-an-email", "Pass1234!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

/// <summary>
/// Minimal fake JWT service that returns a valid-looking token response.
/// Avoids the need for actual JWT configuration in unit tests.
/// </summary>
internal class FakeJwtTokenService : Scheduly.Application.Common.Interfaces.IJwtTokenService
{
    public Scheduly.Application.Common.Models.TokenResponse GenerateTokens(Scheduly.Domain.Entities.User user)
    {
        var now = DateTime.UtcNow;
        return new Scheduly.Application.Common.Models.TokenResponse(
            AccessToken: $"fake-access-{user.Id}",
            RefreshToken: $"fake-refresh-{Guid.NewGuid()}",
            ExpiresAt: now.AddMinutes(15));
    }
}
