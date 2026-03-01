using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Users.Commands.DeactivateUser;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Users;

public class DeactivateUserCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public DeactivateUserCommandHandlerTests()
    {
        _tenantService = new CurrentTenantService();
        _tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);

        SeedData();
    }

    private void SeedData()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test Clinic", Slug = "test-clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = _userId, TenantId = _tenantId, FullName = "Dr. Test",
            Email = "dr@test.com", PasswordHash = "hash", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ExistingUser_SetsIsActiveFalse()
    {
        var handler = new DeactivateUserCommandHandler(_context, new DateTimeProvider());
        var command = new DeactivateUserCommand(_userId);

        await handler.Handle(command, CancellationToken.None);

        var user = await _context.Users.FirstAsync(u => u.Id == _userId);
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AlreadyInactive_StillSucceeds()
    {
        var user = await _context.Users.FirstAsync(u => u.Id == _userId);
        user.IsActive = false;
        await _context.SaveChangesAsync();

        var handler = new DeactivateUserCommandHandler(_context, new DateTimeProvider());
        var command = new DeactivateUserCommand(_userId);

        await handler.Handle(command, CancellationToken.None);

        var updated = await _context.Users.FirstAsync(u => u.Id == _userId);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new DeactivateUserCommandHandler(_context, new DateTimeProvider());
        var command = new DeactivateUserCommand(Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
