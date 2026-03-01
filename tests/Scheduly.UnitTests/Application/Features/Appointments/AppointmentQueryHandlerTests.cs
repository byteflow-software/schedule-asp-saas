using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Appointments.Queries.GetAppointments;
using Scheduly.Application.Features.Appointments.Queries.GetAppointmentById;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class AppointmentQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId1 = Guid.NewGuid();
    private readonly Guid _userId2 = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly Guid _appt1Id = Guid.NewGuid();
    private readonly Guid _appt2Id = Guid.NewGuid();
    private readonly Guid _appt3Id = Guid.NewGuid();

    public AppointmentQueryHandlerTests()
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
            Id = _userId1, TenantId = _tenantId, FullName = "Dr. Alice",
            Email = "alice@test.com", PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = _userId2, TenantId = _tenantId, FullName = "Dr. Bob",
            Email = "bob@test.com", PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customerId, TenantId = _tenantId, FullName = "John Doe",
            Email = "john@test.com", CpfCnpj = "12345678900",
            CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = _serviceId, TenantId = _tenantId, Name = "Check-up",
            DurationMinutes = 60, PriceInCents = 5000, IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appt1Id, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId1, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appt2Id, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId1, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = new DateTime(2025, 7, 15, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 7, 15, 15, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appt3Id, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId2, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 8, 20, 11, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAppointments_ReturnsAllForTenant()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var query = new GetAppointmentsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAppointments_FilterByDateRange()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var query = new GetAppointmentsQuery(
            From: new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            To: new DateTime(2025, 7, 31, 23, 59, 59, DateTimeKind.Utc));

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.First().Id.Should().Be(_appt2Id);
    }

    [Fact]
    public async Task GetAppointments_FilterByUserId()
    {
        var handler = new GetAppointmentsQueryHandler(_context);
        var query = new GetAppointmentsQuery(UserId: _userId1);

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(a => a.UserId.Should().Be(_userId1));
    }

    [Fact]
    public async Task GetAppointments_Pagination()
    {
        // Add 2 more appointments to have 5 total
        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId1, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = new DateTime(2025, 9, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 9, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });
        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId2, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new GetAppointmentsQueryHandler(_context);
        var query = new GetAppointmentsQuery(PageNumber: 1, PageSize: 2);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAppointmentById_ValidId_ReturnsDto()
    {
        var handler = new GetAppointmentByIdQueryHandler(_context);
        var query = new GetAppointmentByIdQuery(_appt1Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(_appt1Id);
        result.CustomerId.Should().Be(_customerId);
        result.CustomerName.Should().Be("John Doe");
        result.UserId.Should().Be(_userId1);
        result.UserName.Should().Be("Dr. Alice");
        result.ServiceId.Should().Be(_serviceId);
        result.ServiceName.Should().Be("Check-up");
        result.PriceInCents.Should().Be(5000);
        result.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task GetAppointmentById_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new GetAppointmentByIdQueryHandler(_context);
        var query = new GetAppointmentByIdQuery(Guid.NewGuid());

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
