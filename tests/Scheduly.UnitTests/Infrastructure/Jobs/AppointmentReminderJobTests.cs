using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Scheduly.Application.Features.Notifications.Commands.SendReminder;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.Jobs;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Jobs;

public class AppointmentReminderJobTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly AppointmentReminderJob _sut;
    private readonly Guid _tenantId = Guid.NewGuid();

    public AppointmentReminderJobTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        _mediator = Substitute.For<IMediator>();

        _sut = new AppointmentReminderJob(
            _context, _mediator, new DateTimeProvider(),
            NullLogger<AppointmentReminderJob>.Instance);

        SeedData();
    }

    private void SeedData()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId, TenantId = _tenantId, FullName = "Dr. Test",
            Email = "dr@test.com", PasswordHash = "hash",
            Role = UserRole.Admin, IsActive = true, CreatedAt = DateTime.UtcNow
        });

        var customerId = Guid.NewGuid();
        _context.Customers.Add(new Customer
        {
            Id = customerId, TenantId = _tenantId, FullName = "Patient",
            Email = "patient@test.com", CpfCnpj = "12345678901", CreatedAt = DateTime.UtcNow
        });

        var serviceId = Guid.NewGuid();
        _context.Services.Add(new Service
        {
            Id = serviceId, TenantId = _tenantId, Name = "Checkup",
            DurationMinutes = 30, PriceInCents = 5000,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        // Appointment within reminder window (next 30 minutes)
        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantId,
            CustomerId = customerId, UserId = userId, ServiceId = serviceId,
            PriceInCents = 5000,
            StartTime = DateTime.UtcNow.AddMinutes(15),
            EndTime = DateTime.UtcNow.AddMinutes(45),
            Status = AppointmentStatus.Confirmed,
            ReminderSentAt = null,
            CreatedAt = DateTime.UtcNow
        });

        // Appointment already reminded
        _context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(), TenantId = _tenantId,
            CustomerId = customerId, UserId = userId, ServiceId = serviceId,
            PriceInCents = 5000,
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            Status = AppointmentStatus.Confirmed,
            ReminderSentAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task ExecuteAsync_SendsRemindersForUpcomingAppointments()
    {
        await _sut.ExecuteAsync();

        await _mediator.Received(1).Send(
            Arg.Any<SendReminderCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MediatorThrows_ContinuesWithoutThrowing()
    {
        _mediator.Send(Arg.Any<SendReminderCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Mediator failed"));

        var act = () => _sut.ExecuteAsync();

        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
