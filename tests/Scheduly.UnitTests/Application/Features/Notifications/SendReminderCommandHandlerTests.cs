using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Notifications.Commands.SendReminder;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Notifications;

public class SendReminderCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();
    private readonly TrackingEmailService _emailService = new();
    private readonly Guid _appointmentId = Guid.NewGuid();

    public SendReminderCommandHandlerTests()
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
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Clinic", Slug = "clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = userId, TenantId = _tenantId, FullName = "Dr. Smith",
            Email = "smith@clinic.com", PasswordHash = "hash",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = customerId, TenantId = _tenantId, FullName = "Jane Doe",
            Email = "jane@example.com", CpfCnpj = "12345678901",
            CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = serviceId, TenantId = _tenantId, Name = "Consultation",
            DurationMinutes = 30, PriceInCents = 5000,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appointmentId, TenantId = _tenantId, CustomerId = customerId,
            UserId = userId, ServiceId = serviceId, PriceInCents = 5000,
            StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidAppointment_SendsEmailAndSetsReminderSentAt()
    {
        var handler = new SendReminderCommandHandler(_context, _emailService, _dateTimeProvider);

        await handler.Handle(new SendReminderCommand(_appointmentId), CancellationToken.None);

        _emailService.ReminderSent.Should().BeTrue();
        _emailService.SentTo.Should().Be("jane@example.com");

        var appointment = await _context.Appointments
            .IgnoreQueryFilters()
            .FirstAsync(a => a.Id == _appointmentId);
        appointment.ReminderSentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentAppointment_ThrowsEntityNotFound()
    {
        var handler = new SendReminderCommandHandler(_context, _emailService, _dateTimeProvider);

        var act = () => handler.Handle(new SendReminderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private class TrackingEmailService : IEmailService
    {
        public bool ReminderSent { get; private set; }
        public string? SentTo { get; private set; }

        public Task SendChargeEmailAsync(
            string customerEmail, string customerName, int amountInCents,
            string referenceNumber, DateTime appointmentDate, string serviceName,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SendReminderAsync(
            string customerEmail, string customerName, DateTime appointmentTime,
            CancellationToken cancellationToken = default)
        {
            ReminderSent = true;
            SentTo = customerEmail;
            return Task.CompletedTask;
        }
    }
}
