using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Appointments.Commands.CancelAppointment;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class CancelAppointmentBranchTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();

    public CancelAppointmentBranchTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        SeedBase();
    }

    private void SeedBase()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
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
            Email = "patient@test.com", CpfCnpj = "12345678901", CreatedAt = DateTime.UtcNow
        });
        _context.Services.Add(new Service
        {
            Id = _serviceId, TenantId = _tenantId, Name = "Svc",
            DurationMinutes = 30, PriceInCents = 5000, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Cancel_WithLinkedTransaction_CancelsTransaction()
    {
        var apptId = Guid.NewGuid();
        var txId = Guid.NewGuid();

        _context.Appointments.Add(new Appointment
        {
            Id = apptId, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });
        _context.Transactions.Add(new Transaction
        {
            Id = txId, TenantId = _tenantId, AppointmentId = apptId,
            CustomerId = _customerId, ReferenceNumber = "TX-001",
            AmountInCents = 5000, Status = TransactionStatus.Pending,
            Type = TransactionType.Charge, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var handler = new CancelAppointmentCommandHandler(_context, new DateTimeProvider());
        await handler.Handle(new CancelAppointmentCommand(apptId), CancellationToken.None);

        var tx = await _context.Transactions.IgnoreQueryFilters().FirstAsync(t => t.Id == txId);
        tx.Status.Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_WithBookedVacancy_FreesVacancy()
    {
        var apptId = Guid.NewGuid();
        var vacancyId = Guid.NewGuid();

        _context.Vacancies.Add(new Vacancy
        {
            Id = vacancyId, TenantId = _tenantId, UserId = _userId, ServiceId = _serviceId,
            StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            IsBooked = true, AppointmentId = apptId, CreatedAt = DateTime.UtcNow
        });
        _context.Appointments.Add(new Appointment
        {
            Id = apptId, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 5000,
            VacancyId = vacancyId,
            StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var handler = new CancelAppointmentCommandHandler(_context, new DateTimeProvider());
        await handler.Handle(new CancelAppointmentCommand(apptId), CancellationToken.None);

        var vacancy = await _context.Vacancies.IgnoreQueryFilters().FirstAsync(v => v.Id == vacancyId);
        vacancy.IsBooked.Should().BeFalse();
        vacancy.AppointmentId.Should().BeNull();
    }

    [Fact]
    public async Task Cancel_WithoutTransaction_CompletesSuccessfully()
    {
        var apptId = Guid.NewGuid();
        _context.Appointments.Add(new Appointment
        {
            Id = apptId, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 5000,
            StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = AppointmentStatus.PendingPayment, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var handler = new CancelAppointmentCommandHandler(_context, new DateTimeProvider());
        await handler.Handle(new CancelAppointmentCommand(apptId), CancellationToken.None);

        var appt = await _context.Appointments.IgnoreQueryFilters().FirstAsync(a => a.Id == apptId);
        appt.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
