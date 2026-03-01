using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Transactions.Commands.CancelTransaction;
using Scheduly.Application.Features.Transactions.Commands.PayTransaction;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Transactions;

public class TransactionCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();
    private readonly Guid _transactionId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();

    public TransactionCommandHandlerTests()
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
            Id = customerId, TenantId = _tenantId, FullName = "John Doe",
            Email = "john@example.com", CpfCnpj = "12345678901",
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
            Status = AppointmentStatus.PendingPayment, CreatedAt = DateTime.UtcNow
        });

        _context.Transactions.Add(new Transaction
        {
            Id = _transactionId, TenantId = _tenantId, AppointmentId = _appointmentId,
            CustomerId = customerId, ReferenceNumber = "REF-001",
            AmountInCents = 5000, Status = TransactionStatus.Pending,
            Type = TransactionType.Charge, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    // ── CancelTransaction ──────────────────────────────────────────────────

    [Fact]
    public async Task CancelTransaction_PendingTransaction_SetsCancelledStatus()
    {
        var handler = new CancelTransactionCommandHandler(_context, _dateTimeProvider);

        await handler.Handle(new CancelTransactionCommand(_transactionId), CancellationToken.None);

        var transaction = await _context.Transactions.FirstAsync(t => t.Id == _transactionId);
        transaction.Status.Should().Be(TransactionStatus.Cancelled);
        transaction.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelTransaction_AlreadyCancelled_ThrowsDomainException()
    {
        // First cancel
        var handler = new CancelTransactionCommandHandler(_context, _dateTimeProvider);
        await handler.Handle(new CancelTransactionCommand(_transactionId), CancellationToken.None);

        // Try to cancel again
        var act = () => handler.Handle(new CancelTransactionCommand(_transactionId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public async Task CancelTransaction_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new CancelTransactionCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new CancelTransactionCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── PayTransaction ─────────────────────────────────────────────────────

    [Fact]
    public async Task PayTransaction_PendingTransaction_SetsPaidStatusAndPaidAt()
    {
        var handler = new PayTransactionCommandHandler(_context, _dateTimeProvider);

        var result = await handler.Handle(
            new PayTransactionCommand(_transactionId, "PIX"), CancellationToken.None);

        result.Status.Should().Be("Paid");
        result.PaidAt.Should().NotBeNull();
        result.PaymentMethod.Should().Be("PIX");
    }

    [Fact]
    public async Task PayTransaction_AutoConfirmsLinkedPendingPaymentAppointment()
    {
        var handler = new PayTransactionCommandHandler(_context, _dateTimeProvider);

        await handler.Handle(
            new PayTransactionCommand(_transactionId, "PIX"), CancellationToken.None);

        var appointment = await _context.Appointments.FirstAsync(a => a.Id == _appointmentId);
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PayTransaction_AlreadyPaid_ThrowsDomainException()
    {
        var handler = new PayTransactionCommandHandler(_context, _dateTimeProvider);

        // First pay
        await handler.Handle(
            new PayTransactionCommand(_transactionId, "PIX"), CancellationToken.None);

        // Try to pay again
        var act = () => handler.Handle(
            new PayTransactionCommand(_transactionId, "PIX"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*pending*");
    }

    [Fact]
    public async Task PayTransaction_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new PayTransactionCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(
            new PayTransactionCommand(Guid.NewGuid(), "PIX"), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
