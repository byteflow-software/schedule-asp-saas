using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Appointments.Commands.UpdateAppointment;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class UpdateAppointmentCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly Guid _appointmentId1 = Guid.NewGuid();
    private readonly Guid _appointmentId2 = Guid.NewGuid();

    public UpdateAppointmentCommandHandlerTests()
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

        var baseDate = DateTime.UtcNow.AddDays(1).Date;

        _context.Appointments.Add(new Appointment
        {
            Id = _appointmentId1, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = baseDate.AddHours(10),
            EndTime = baseDate.AddHours(11),
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appointmentId2, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = baseDate.AddHours(14),
            EndTime = baseDate.AddHours(15),
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesFieldsAndReturnsDto()
    {
        var handler = new UpdateAppointmentCommandHandler(_context, new DateTimeProvider());
        var newStart = DateTime.UtcNow.AddDays(3).Date.AddHours(9);
        var newEnd = DateTime.UtcNow.AddDays(3).Date.AddHours(10);
        var command = new UpdateAppointmentCommand(_appointmentId1, newStart, newEnd, "Updated notes");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.StartTime.Should().Be(newStart);
        result.EndTime.Should().Be(newEnd);
        result.Notes.Should().Be("Updated notes");

        var updated = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId1);
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_OverlappingTime_ThrowsOverlapException()
    {
        var handler = new UpdateAppointmentCommandHandler(_context, new DateTimeProvider());
        var baseDate = DateTime.UtcNow.AddDays(1).Date;

        // Update appointment2 into appointment1's time slot
        var command = new UpdateAppointmentCommand(
            _appointmentId2,
            baseDate.AddHours(10).AddMinutes(15),
            baseDate.AddHours(11).AddMinutes(15),
            null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AppointmentOverlapException>();
    }

    [Fact]
    public async Task Handle_CancelledStatus_ThrowsDomainException()
    {
        var appointment = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId1);
        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        var handler = new UpdateAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new UpdateAppointmentCommand(
            _appointmentId1,
            DateTime.UtcNow.AddDays(5).Date.AddHours(10),
            DateTime.UtcNow.AddDays(5).Date.AddHours(11),
            null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task Handle_CompletedStatus_ThrowsDomainException()
    {
        var appointment = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId1);
        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();

        var handler = new UpdateAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new UpdateAppointmentCommand(
            _appointmentId1,
            DateTime.UtcNow.AddDays(5).Date.AddHours(10),
            DateTime.UtcNow.AddDays(5).Date.AddHours(11),
            null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Code == "INVALID_STATUS");
    }

    [Fact]
    public async Task Handle_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new UpdateAppointmentCommandHandler(_context, new DateTimeProvider());
        var command = new UpdateAppointmentCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
