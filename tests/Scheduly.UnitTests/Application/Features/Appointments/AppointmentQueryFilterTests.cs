using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Appointments.Queries.GetAppointments;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class AppointmentQueryFilterTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _user1Id = Guid.NewGuid();
    private readonly Guid _user2Id = Guid.NewGuid();
    private readonly Guid _customer1Id = Guid.NewGuid();
    private readonly Guid _customer2Id = Guid.NewGuid();

    public AppointmentQueryFilterTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        SeedData();
    }

    private void SeedData()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = _user1Id, TenantId = _tenantId, FullName = "Dr. One",
            Email = "one@test.com", PasswordHash = "h", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Users.Add(new User
        {
            Id = _user2Id, TenantId = _tenantId, FullName = "Dr. Two",
            Email = "two@test.com", PasswordHash = "h", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customer1Id, TenantId = _tenantId, FullName = "C1",
            Email = "c1@test.com", CpfCnpj = "11111111111", CreatedAt = DateTime.UtcNow
        });
        _context.Customers.Add(new Customer
        {
            Id = _customer2Id, TenantId = _tenantId, FullName = "C2",
            Email = "c2@test.com", CpfCnpj = "22222222222", CreatedAt = DateTime.UtcNow
        });

        var serviceId = Guid.NewGuid();
        _context.Services.Add(new Service
        {
            Id = serviceId, TenantId = _tenantId, Name = "Svc",
            DurationMinutes = 30, PriceInCents = 5000, IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantId,
            CustomerId = _customer1Id, UserId = _user1Id, ServiceId = serviceId,
            PriceInCents = 5000,
            StartTime = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });
        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantId,
            CustomerId = _customer2Id, UserId = _user2Id, ServiceId = serviceId,
            PriceInCents = 5000,
            StartTime = new DateTime(2025, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task FilterByFrom_ReturnsOnlyAfterDate()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetAppointmentsQuery(From: new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task FilterByTo_ReturnsOnlyBeforeDate()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetAppointmentsQuery(To: new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task FilterByUserId_ReturnsOnlyMatchingUser()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetAppointmentsQuery(UserId: _user1Id),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.First().UserName.Should().Be("Dr. One");
    }

    [Fact]
    public async Task FilterByCustomerId_ReturnsOnlyMatchingCustomer()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetAppointmentsQuery(CustomerId: _customer2Id),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.First().CustomerName.Should().Be("C2");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
