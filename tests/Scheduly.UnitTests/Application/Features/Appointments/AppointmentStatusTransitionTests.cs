using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Appointments.Commands.CancelAppointment;
using Scheduly.Application.Features.Appointments.Commands.CompleteAppointment;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Appointments;

/// <summary>
/// Tests for Cancel and Complete appointment status transitions,
/// including invalid state transitions.
/// </summary>
public class AppointmentStatusTransitionTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();

    public AppointmentStatusTransitionTests()
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
            Id = _tenantId, Name = "Clinic", Slug = "clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Users.Add(new User
        {
            Id = _userId, TenantId = _tenantId, FullName = "Dr. Test",
            Email = "dr@test.com", PasswordHash = "h", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Customers.Add(new Customer
        {
            Id = _customerId, TenantId = _tenantId, FullName = "Patient",
            Email = "patient@test.com", CreatedAt = DateTime.UtcNow
        });
        _context.Services.Add(new Service
        {
            Id = _serviceId, TenantId = _tenantId, Name = "Consultation",
            DurationMinutes = 60, PriceInCents = 10000, IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    private Guid SeedAppointment(AppointmentStatus status = AppointmentStatus.PendingPayment)
    {
        var id = Guid.NewGuid();
        _context.Appointments.Add(new Appointment
        {
            Id = id, TenantId = _tenantId, CustomerId = _customerId, UserId = _userId,
            ServiceId = _serviceId, PriceInCents = 10000,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = status, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
        return id;
    }

    // ── Cancel ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAppointment_PendingPayment_SetsStatusToCancelled()
    {
        var id = SeedAppointment(AppointmentStatus.PendingPayment);
        var handler = new CancelAppointmentCommandHandler(_context, _dateTimeProvider);

        await handler.Handle(new CancelAppointmentCommand(id), CancellationToken.None);

        var updated = await _context.Appointments.IgnoreQueryFilters().FirstAsync(a => a.Id == id);
        updated.Status.Should().Be(AppointmentStatus.Cancelled);
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelAppointment_Confirmed_SetsStatusToCancelled()
    {
        var id = SeedAppointment(AppointmentStatus.Confirmed);
        var handler = new CancelAppointmentCommandHandler(_context, _dateTimeProvider);

        await handler.Handle(new CancelAppointmentCommand(id), CancellationToken.None);

        var updated = await _context.Appointments.IgnoreQueryFilters().FirstAsync(a => a.Id == id);
        updated.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAppointment_AlreadyCancelled_ThrowsDomainException()
    {
        var id = SeedAppointment(AppointmentStatus.Cancelled);
        var handler = new CancelAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CancelAppointmentCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task CancelAppointment_Completed_ThrowsDomainException()
    {
        var id = SeedAppointment(AppointmentStatus.Completed);
        var handler = new CancelAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CancelAppointmentCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task CancelAppointment_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new CancelAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CancelAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── Complete ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteAppointment_Confirmed_SetsStatusToCompleted()
    {
        var id = SeedAppointment(AppointmentStatus.Confirmed);
        var handler = new CompleteAppointmentCommandHandler(_context, _dateTimeProvider);

        await handler.Handle(new CompleteAppointmentCommand(id), CancellationToken.None);

        var updated = await _context.Appointments.IgnoreQueryFilters().FirstAsync(a => a.Id == id);
        updated.Status.Should().Be(AppointmentStatus.Completed);
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteAppointment_PendingPayment_ThrowsDomainException()
    {
        var id = SeedAppointment(AppointmentStatus.PendingPayment);
        var handler = new CompleteAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CompleteAppointmentCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task CompleteAppointment_AlreadyCompleted_ThrowsDomainException()
    {
        var id = SeedAppointment(AppointmentStatus.Completed);
        var handler = new CompleteAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CompleteAppointmentCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task CompleteAppointment_Cancelled_ThrowsDomainException()
    {
        var id = SeedAppointment(AppointmentStatus.Cancelled);
        var handler = new CompleteAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CompleteAppointmentCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task CompleteAppointment_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new CompleteAppointmentCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CompleteAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
