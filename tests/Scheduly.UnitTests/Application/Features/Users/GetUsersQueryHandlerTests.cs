using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Users.Queries.GetUsers;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Users;

public class GetUsersQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _otherTenantId = Guid.NewGuid();

    public GetUsersQueryHandlerTests()
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

        _context.Tenants.Add(new Tenant
        {
            Id = _otherTenantId, Name = "Other Clinic", Slug = "other-clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, FullName = "Charlie",
            Email = "charlie@test.com", PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, FullName = "Alice",
            Email = "alice@test.com", PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, FullName = "Bob",
            Email = "bob@test.com", PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        // Other tenant user - should NOT appear in results
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(), TenantId = _otherTenantId, FullName = "Other User",
            Email = "other@test.com", PasswordHash = "hash", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetUsers_ReturnsOrderedByFullName()
    {
        var handler = new GetUsersQueryHandler(_context);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result[0].FullName.Should().Be("Alice");
        result[1].FullName.Should().Be("Bob");
        result[2].FullName.Should().Be("Charlie");
    }

    [Fact]
    public async Task GetUsers_OnlyReturnsTenantUsers()
    {
        var handler = new GetUsersQueryHandler(_context);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().NotContain(u => u.FullName == "Other User");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
