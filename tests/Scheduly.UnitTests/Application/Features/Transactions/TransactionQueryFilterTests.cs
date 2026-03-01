using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Transactions.Queries.GetTransactions;
using Scheduly.Application.Features.Transactions.Queries.GetTransactionSummary;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Transactions;

public class TransactionQueryFilterTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customer1Id = Guid.NewGuid();
    private readonly Guid _customer2Id = Guid.NewGuid();

    public TransactionQueryFilterTests()
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

        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId, TenantId = _tenantId, FullName = "Dr. Test",
            Email = "dr@test.com", PasswordHash = "h", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customer1Id, TenantId = _tenantId, FullName = "Customer 1",
            Email = "c1@test.com", CpfCnpj = "11111111111", CreatedAt = DateTime.UtcNow
        });
        _context.Customers.Add(new Customer
        {
            Id = _customer2Id, TenantId = _tenantId, FullName = "Customer 2",
            Email = "c2@test.com", CpfCnpj = "22222222222", CreatedAt = DateTime.UtcNow
        });

        var serviceId = Guid.NewGuid();
        _context.Services.Add(new Service
        {
            Id = serviceId, TenantId = _tenantId, Name = "Svc",
            DurationMinutes = 30, PriceInCents = 5000, IsActive = true, CreatedAt = DateTime.UtcNow
        });

        var appt1Id = Guid.NewGuid();
        var appt2Id = Guid.NewGuid();
        _context.Appointments.Add(new Appointment
        {
            Id = appt1Id, TenantId = _tenantId, CustomerId = _customer1Id,
            UserId = userId, ServiceId = serviceId, PriceInCents = 5000,
            StartTime = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });
        _context.Appointments.Add(new Appointment
        {
            Id = appt2Id, TenantId = _tenantId, CustomerId = _customer2Id,
            UserId = userId, ServiceId = serviceId, PriceInCents = 3000,
            StartTime = new DateTime(2025, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Confirmed, CreatedAt = DateTime.UtcNow
        });

        _context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, AppointmentId = appt1Id,
            CustomerId = _customer1Id, ReferenceNumber = "TX-001",
            AmountInCents = 5000, Status = TransactionStatus.Paid,
            Type = TransactionType.Charge,
            CreatedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        _context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, AppointmentId = appt2Id,
            CustomerId = _customer2Id, ReferenceNumber = "TX-002",
            AmountInCents = 3000, Status = TransactionStatus.Pending,
            Type = TransactionType.Charge,
            CreatedAt = new DateTime(2025, 7, 1, 12, 0, 0, DateTimeKind.Utc)
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTransactions_FilterByFrom_ReturnsAfterDate()
    {
        var handler = new GetTransactionsQueryHandler(_context);
        var result = await handler.Handle(
            new GetTransactionsQuery(From: new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetTransactions_FilterByTo_ReturnsBeforeDate()
    {
        var handler = new GetTransactionsQueryHandler(_context);
        var result = await handler.Handle(
            new GetTransactionsQuery(To: new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetTransactions_FilterByCustomerId_ReturnsOnlyMatching()
    {
        var handler = new GetTransactionsQueryHandler(_context);
        var result = await handler.Handle(
            new GetTransactionsQuery(CustomerId: _customer1Id),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.First().CustomerName.Should().Be("Customer 1");
    }

    [Fact]
    public async Task GetTransactions_FilterByInvalidStatus_ReturnsAll()
    {
        var handler = new GetTransactionsQueryHandler(_context);
        var result = await handler.Handle(
            new GetTransactionsQuery(Status: "InvalidStatus"),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetTransactionSummary_FilterByFrom()
    {
        var handler = new GetTransactionSummaryQueryHandler(_context);
        var result = await handler.Handle(
            new GetTransactionSummaryQuery(From: new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.Count.Should().Be(1);
        result.TotalPendingCents.Should().Be(3000);
    }

    [Fact]
    public async Task GetTransactionSummary_FilterByTo()
    {
        var handler = new GetTransactionSummaryQueryHandler(_context);
        var result = await handler.Handle(
            new GetTransactionSummaryQuery(To: new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.Count.Should().Be(1);
        result.TotalPaidCents.Should().Be(5000);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
