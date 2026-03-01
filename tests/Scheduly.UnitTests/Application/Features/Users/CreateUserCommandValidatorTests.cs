using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Users.Commands.CreateUser;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Users;

public class CreateUserCommandValidatorTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateUserCommandValidator _sut;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateUserCommandValidatorTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        _sut = new CreateUserCommandValidator(_context);

        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Validate_ValidCommand_IsValid()
    {
        var cmd = new CreateUserCommand("John Doe", "john@test.com", "Password123!", "Admin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyName_IsInvalid()
    {
        var cmd = new CreateUserCommand("", "john@test.com", "Password123!", "Admin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public async Task Validate_NameTooLong_IsInvalid()
    {
        var cmd = new CreateUserCommand(new string('A', 201), "john@test.com", "Password123!", "Admin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_InvalidEmail_IsInvalid()
    {
        var cmd = new CreateUserCommand("John", "not-email", "Password123!", "Admin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_DuplicateEmail_IsInvalid()
    {
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, FullName = "Existing",
            Email = "existing@test.com", PasswordHash = "hash",
            Role = UserRole.Admin, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var cmd = new CreateUserCommand("John", "existing@test.com", "Password123!", "Admin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("already in use"));
    }

    [Fact]
    public async Task Validate_ShortPassword_IsInvalid()
    {
        var cmd = new CreateUserCommand("John", "john@test.com", "short", "Admin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_InvalidRole_IsInvalid()
    {
        var cmd = new CreateUserCommand("John", "john@test.com", "Password123!", "SuperAdmin");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Staff")]
    [InlineData("admin")]
    [InlineData("staff")]
    public async Task Validate_ValidRole_IsValid(string role)
    {
        var cmd = new CreateUserCommand("John", "john@test.com", "Password123!", role);
        var result = await _sut.ValidateAsync(cmd);
        result.Errors.Should().NotContain(e => e.PropertyName == "Role");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
