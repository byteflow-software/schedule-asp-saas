using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Appointments.Commands.ConfirmAppointment;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class ConfirmAppointmentCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();

    public ConfirmAppointmentCommandHandlerTests()
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

        _context.Customers.Add(new Customer
        {
            Id = _customerId, TenantId = _tenantId, FullName = "John Doe",
            Email = "john@test.com", CpfCnpj = "12345678901", CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = _serviceId, TenantId = _tenantId, Name = "Check-up",
            DurationMinutes = 60, PriceInCents = 5000, IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appointmentId, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(11),
            Status = AppointmentStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_PendingPayment_SetsStatusToConfirmed()
    {
        var handler = new ConfirmAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new ConfirmAppointmentCommand(_appointmentId);

        await handler.Handle(command, CancellationToken.None);

        var appointment = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId);
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AlreadyConfirmed_ThrowsDomainException()
    {
        var appointment = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId);
        appointment.Status = AppointmentStatus.Confirmed;
        await _context.SaveChangesAsync();

        var handler = new ConfirmAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new ConfirmAppointmentCommand(_appointmentId);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task Handle_CancelledAppointment_ThrowsDomainException()
    {
        var appointment = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId);
        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        var handler = new ConfirmAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new ConfirmAppointmentCommand(_appointmentId);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task Handle_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new ConfirmAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new ConfirmAppointmentCommand(Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
