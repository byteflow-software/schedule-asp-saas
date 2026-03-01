using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Transactions.Queries.GetTransactions;
using Scheduly.Application.Features.Transactions.Queries.GetTransactionById;
using Scheduly.Application.Features.Transactions.Queries.GetTransactionSummary;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Transactions;

public class TransactionQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly Guid _appt1Id = Guid.NewGuid();
    private readonly Guid _appt2Id = Guid.NewGuid();
    private readonly Guid _appt3Id = Guid.NewGuid();
    private readonly Guid _txPendingId = Guid.NewGuid();
    private readonly Guid _txPaidId = Guid.NewGuid();
    private readonly Guid _txCancelledId = Guid.NewGuid();

    public TransactionQueryHandlerTests()
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
            Id = _userId, TenantId = _tenantId, FullName = "Dr. Alice",
            Email = "alice@test.com", PasswordHash = "hash", Role = UserRole.Staff,
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
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 1000,
            StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appt2Id, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 2000,
            StartTime = DateTime.UtcNow.AddDays(2), EndTime = DateTime.UtcNow.AddDays(2).AddHours(1),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.Appointments.Add(new Appointment
        {
            Id = _appt3Id, TenantId = _tenantId, CustomerId = _customerId,
            UserId = _userId, ServiceId = _serviceId, PriceInCents = 500,
            StartTime = DateTime.UtcNow.AddDays(3), EndTime = DateTime.UtcNow.AddDays(3).AddHours(1),
            Status = AppointmentStatus.Cancelled, CreatedAt = DateTime.UtcNow
        });

        _context.Transactions.Add(new Transaction
        {
            Id = _txPendingId, TenantId = _tenantId, AppointmentId = _appt1Id,
            CustomerId = _customerId, ReferenceNumber = "REF-001",
            AmountInCents = 1000, Status = TransactionStatus.Pending,
            Type = TransactionType.Charge, CreatedAt = DateTime.UtcNow
        });

        _context.Transactions.Add(new Transaction
        {
            Id = _txPaidId, TenantId = _tenantId, AppointmentId = _appt2Id,
            CustomerId = _customerId, ReferenceNumber = "REF-002",
            AmountInCents = 2000, Status = TransactionStatus.Paid,
            Type = TransactionType.Charge, PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        _context.Transactions.Add(new Transaction
        {
            Id = _txCancelledId, TenantId = _tenantId, AppointmentId = _appt3Id,
            CustomerId = _customerId, ReferenceNumber = "REF-003",
            AmountInCents = 500, Status = TransactionStatus.Cancelled,
            Type = TransactionType.Charge, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTransactions_ReturnsAll()
    {
        var handler = new GetTransactionsQueryHandler(_context);
        var query = new GetTransactionsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTransactions_FilterByStatus()
    {
        var handler = new GetTransactionsQueryHandler(_context);
        var query = new GetTransactionsQuery(Status: "Paid");

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.First().Status.Should().Be("Paid");
        result.Items.First().Id.Should().Be(_txPaidId);
    }

    [Fact]
    public async Task GetTransactionById_ValidId_ReturnsDto()
    {
        var handler = new GetTransactionByIdQueryHandler(_context);
        var query = new GetTransactionByIdQuery(_txPaidId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(_txPaidId);
        result.AmountInCents.Should().Be(2000);
        result.Status.Should().Be("Paid");
        result.ReferenceNumber.Should().Be("REF-002");
        result.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetTransactionById_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new GetTransactionByIdQueryHandler(_context);
        var query = new GetTransactionByIdQuery(Guid.NewGuid());

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetTransactionSummary_CalculatesCorrectAggregates()
    {
        var handler = new GetTransactionSummaryQueryHandler(_context);
        var query = new GetTransactionSummaryQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        // TotalRevenue excludes Cancelled: Pending(1000) + Paid(2000) = 3000
        result.TotalRevenueCents.Should().Be(3000);
        result.TotalPendingCents.Should().Be(1000);
        result.TotalPaidCents.Should().Be(2000);
        result.Count.Should().Be(3);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
