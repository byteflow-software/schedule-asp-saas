using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.IntegrationTests.Infrastructure;

/// <summary>
/// Verifies that the global query filters in ApplicationDbContext correctly
/// isolate Services, Vacancies, and Transactions between tenants.
/// </summary>
public class TenantIsolationExtendedTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;

    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();
    private readonly Guid _userA = Guid.NewGuid();
    private readonly Guid _userB = Guid.NewGuid();
    private readonly Guid _customerA = Guid.NewGuid();
    private readonly Guid _customerB = Guid.NewGuid();
    private readonly Guid _serviceA = Guid.NewGuid();
    private readonly Guid _serviceB = Guid.NewGuid();

    public TenantIsolationExtendedTests()
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
        // Tenants
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

        // Users
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

        // Customers
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

        // Services
        _context.Services.Add(new Service
        {
            Id = _serviceA, TenantId = _tenantA, Name = "Service A",
            DurationMinutes = 60, PriceInCents = 5000,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Services.Add(new Service
        {
            Id = _serviceB, TenantId = _tenantB, Name = "Service B",
            DurationMinutes = 30, PriceInCents = 3000,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        // Vacancies
        var startTime = DateTime.UtcNow.AddDays(1);
        _context.Vacancies.Add(new Vacancy
        {
            Id = Guid.NewGuid(), TenantId = _tenantA,
            UserId = _userA, ServiceId = _serviceA,
            StartTime = startTime, EndTime = startTime.AddHours(1),
            IsBooked = false, CreatedAt = DateTime.UtcNow
        });
        _context.Vacancies.Add(new Vacancy
        {
            Id = Guid.NewGuid(), TenantId = _tenantB,
            UserId = _userB, ServiceId = _serviceB,
            StartTime = startTime, EndTime = startTime.AddHours(1),
            IsBooked = false, CreatedAt = DateTime.UtcNow
        });

        // Appointments (needed for transactions)
        var apptA = new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantA,
            CustomerId = _customerA, UserId = _userA, ServiceId = _serviceA,
            StartTime = startTime, EndTime = startTime.AddHours(1),
            PriceInCents = 5000, Status = AppointmentStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };
        var apptB = new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantB,
            CustomerId = _customerB, UserId = _userB, ServiceId = _serviceB,
            StartTime = startTime, EndTime = startTime.AddHours(1),
            PriceInCents = 3000, Status = AppointmentStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };
        _context.Appointments.AddRange(apptA, apptB);

        // Transactions
        _context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(), TenantId = _tenantA,
            AppointmentId = apptA.Id, CustomerId = _customerA,
            ReferenceNumber = "TX-A-001", AmountInCents = 5000,
            Status = TransactionStatus.Pending, Type = TransactionType.Charge,
            CreatedAt = DateTime.UtcNow
        });
        _context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(), TenantId = _tenantB,
            AppointmentId = apptB.Id, CustomerId = _customerB,
            ReferenceNumber = "TX-B-001", AmountInCents = 3000,
            Status = TransactionStatus.Pending, Type = TransactionType.Charge,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Services_QueryFilter_IsolatesPerTenant()
    {
        _tenantService.SetTenantId(_tenantA);
        var servicesA = await _context.Services.ToListAsync();
        servicesA.Should().HaveCount(1);
        servicesA.Single().TenantId.Should().Be(_tenantA);

        _tenantService.SetTenantId(_tenantB);
        var servicesB = await _context.Services.ToListAsync();
        servicesB.Should().HaveCount(1);
        servicesB.Single().TenantId.Should().Be(_tenantB);
    }

    [Fact]
    public async Task Vacancies_QueryFilter_IsolatesPerTenant()
    {
        _tenantService.SetTenantId(_tenantA);
        var vacanciesA = await _context.Vacancies.ToListAsync();
        vacanciesA.Should().HaveCount(1);
        vacanciesA.Should().AllSatisfy(v => v.TenantId.Should().Be(_tenantA));

        _tenantService.SetTenantId(_tenantB);
        var vacanciesB = await _context.Vacancies.ToListAsync();
        vacanciesB.Should().HaveCount(1);
        vacanciesB.Should().AllSatisfy(v => v.TenantId.Should().Be(_tenantB));
    }

    [Fact]
    public async Task Transactions_QueryFilter_IsolatesPerTenant()
    {
        _tenantService.SetTenantId(_tenantA);
        var transactionsA = await _context.Transactions.ToListAsync();
        transactionsA.Should().HaveCount(1);
        transactionsA.Should().AllSatisfy(t => t.TenantId.Should().Be(_tenantA));

        _tenantService.SetTenantId(_tenantB);
        var transactionsB = await _context.Transactions.ToListAsync();
        transactionsB.Should().HaveCount(1);
        transactionsB.Should().AllSatisfy(t => t.TenantId.Should().Be(_tenantB));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
