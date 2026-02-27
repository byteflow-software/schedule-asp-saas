using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Infrastructure.MultiTenancy;

/// <summary>
/// Verifies that the global query filters in ApplicationDbContext correctly
/// isolate data between tenants. A tenant must only see its own Users,
/// Appointments, and Customers.
/// </summary>
public class TenantIsolationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;

    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();
    private readonly Guid _userA = Guid.NewGuid();
    private readonly Guid _userB = Guid.NewGuid();
    private readonly Guid _customerA = Guid.NewGuid();
    private readonly Guid _customerB = Guid.NewGuid();

    public TenantIsolationTests()
    {
        _tenantService = new CurrentTenantService();

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
            Id = _tenantA, Name = "Clinic A", Slug = "clinic-a",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantB, Name = "Clinic B", Slug = "clinic-b",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = _userA, TenantId = _tenantA, FullName = "User A",
            Email = "a@tenantA.com", PasswordHash = "h", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Users.Add(new User
        {
            Id = _userB, TenantId = _tenantB, FullName = "User B",
            Email = "b@tenantB.com", PasswordHash = "h", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customerA, TenantId = _tenantA, FullName = "Customer A",
            Email = "cust@tenantA.com", CreatedAt = DateTime.UtcNow
        });
        _context.Customers.Add(new Customer
        {
            Id = _customerB, TenantId = _tenantB, FullName = "Customer B",
            Email = "cust@tenantB.com", CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantA,
            CustomerId = _customerA, UserId = _userA,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            ServiceId = Guid.NewGuid(), PriceInCents = 5000,
            Status = AppointmentStatus.PendingPayment, CreatedAt = DateTime.UtcNow
        });
        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantB,
            CustomerId = _customerB, UserId = _userB,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            ServiceId = Guid.NewGuid(), PriceInCents = 5000,
            Status = AppointmentStatus.PendingPayment, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Users_QueryFilter_OnlyReturnsTenantAUsers()
    {
        _tenantService.SetTenantId(_tenantA);

        var users = await _context.Users.ToListAsync();

        users.Should().HaveCount(1);
        users.Single().TenantId.Should().Be(_tenantA);
    }

    [Fact]
    public async Task Users_QueryFilter_OnlyReturnsTenantBUsers()
    {
        _tenantService.SetTenantId(_tenantB);

        var users = await _context.Users.ToListAsync();

        users.Should().HaveCount(1);
        users.Single().TenantId.Should().Be(_tenantB);
    }

    [Fact]
    public async Task Appointments_QueryFilter_IsolatesPerTenant()
    {
        _tenantService.SetTenantId(_tenantA);
        var apptA = await _context.Appointments.ToListAsync();
        apptA.Should().AllSatisfy(a => a.TenantId.Should().Be(_tenantA));

        _tenantService.SetTenantId(_tenantB);
        var apptB = await _context.Appointments.ToListAsync();
        apptB.Should().AllSatisfy(a => a.TenantId.Should().Be(_tenantB));
    }

    [Fact]
    public async Task Customers_QueryFilter_IsolatesPerTenant()
    {
        _tenantService.SetTenantId(_tenantA);
        var customersA = await _context.Customers.ToListAsync();
        customersA.Should().HaveCount(1).And.AllSatisfy(c => c.TenantId.Should().Be(_tenantA));
    }

    [Fact]
    public async Task Customers_SoftDeleted_AreHiddenByQueryFilter()
    {
        _tenantService.SetTenantId(_tenantA);

        // Soft-delete customerA
        var cust = await _context.Customers.FirstAsync(c => c.Id == _customerA);
        cust.IsDeleted = true;
        cust.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Should be invisible through normal query
        var visible = await _context.Customers.ToListAsync();
        visible.Should().BeEmpty();

        // But still present when IgnoreQueryFilters is used
        var inDb = await _context.Customers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == _customerA);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task IgnoreQueryFilters_ReturnsAllTenantsData()
    {
        // Regardless of current tenant, IgnoreQueryFilters should see all
        _tenantService.SetTenantId(_tenantA);

        var allUsers = await _context.Users.IgnoreQueryFilters().ToListAsync();

        allUsers.Should().HaveCount(2,
            because: "IgnoreQueryFilters bypasses the tenant isolation filter");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
