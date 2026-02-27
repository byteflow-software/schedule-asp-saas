using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Users.Commands.CreateUser;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.Identity;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Users;

public class CreateUserCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly PasswordHasherService _hasher = new();
    private readonly DateTimeProvider _dateTimeProvider = new();

    public CreateUserCommandHandlerTests()
    {
        _tenantService = new CurrentTenantService();
        _tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);

        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Clinic", Slug = "clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    private CreateUserCommandHandler CreateHandler() =>
        new(_context, _tenantService, _hasher, _dateTimeProvider);

    [Fact]
    public async Task Handle_ValidCommand_CreatesUser()
    {
        var command = new CreateUserCommand("Jane Staff", "jane@clinic.com", "Password1!", "Staff");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FullName.Should().Be("Jane Staff");
        result.Email.Should().Be("jane@clinic.com");
        result.Role.Should().Be("Staff");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRole_ThrowsArgumentException()
    {
        var act = () => CreateHandler().Handle(
            new CreateUserCommand("User", "u@clinic.com", "Password1!", "SuperAdmin"),
            CancellationToken.None);

        // Enum.Parse throws ArgumentException for unknown role values
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_PasswordIsHashed()
    {
        var plainPassword = "Secure1234!";
        var result = await CreateHandler().Handle(
            new CreateUserCommand("Jane", "jane@c.com", plainPassword, "Staff"),
            CancellationToken.None);

        var saved = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == result.Id);
        saved.PasswordHash.Should().NotBe(plainPassword);
        new PasswordHasherService().Verify(plainPassword, saved.PasswordHash).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
