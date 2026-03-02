using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Scheduly.Application.Features.Appointments.Commands.CreateAppointment;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;
using Scheduly.UnitTests.Common;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class CreateAppointmentBranchTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();

    public CreateAppointmentBranchTests()
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
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, AsaasApiKey = "$aact_hmlg_test",
            CreatedAt = DateTime.UtcNow
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
    public async Task Handle_WithVacancyId_BooksVacancy()
    {
        var vacancyId = Guid.NewGuid();
        _context.Vacancies.Add(new Vacancy
        {
            Id = vacancyId, TenantId = _tenantId, UserId = _userId, ServiceId = _serviceId,
            StartTime = DateTime.UtcNow.AddDays(3), EndTime = DateTime.UtcNow.AddDays(3).AddMinutes(30),
            IsBooked = false, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var handler = CreateHandler();
        var result = await handler.Handle(new CreateAppointmentCommand(
            _customerId, _userId, _serviceId, vacancyId,
            DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(3).AddMinutes(30), null),
            CancellationToken.None);

        result.Should().NotBeNull();

        var vacancy = await _context.Vacancies.IgnoreQueryFilters().FirstAsync(v => v.Id == vacancyId);
        vacancy.IsBooked.Should().BeTrue();
        vacancy.AppointmentId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentService_ThrowsEntityNotFound()
    {
        var handler = CreateHandler();
        var act = () => handler.Handle(new CreateAppointmentCommand(
            _customerId, _userId, Guid.NewGuid(), null,
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(1), null),
            CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_NonExistentCustomer_ThrowsEntityNotFound()
    {
        var handler = CreateHandler();
        var act = () => handler.Handle(new CreateAppointmentCommand(
            Guid.NewGuid(), _userId, _serviceId, null,
            DateTime.UtcNow.AddDays(6), DateTime.UtcNow.AddDays(6).AddHours(1), null),
            CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_AsaasServiceFails_StillCreatesAppointment()
    {
        var handler = new CreateAppointmentCommandHandler(
            _context, _tenantService, new DateTimeProvider(),
            new StubEmailService(), new FailingAsaasService(),
            new StubErrorLogService(),
            NullLogger<CreateAppointmentCommandHandler>.Instance);

        var result = await handler.Handle(new CreateAppointmentCommand(
            _customerId, _userId, _serviceId, null,
            DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(10).AddHours(1), "notes"),
            CancellationToken.None);

        // Should succeed even if Asaas fails (catch block in TryCreateAsaasPaymentAsync)
        result.Should().NotBeNull();
        result.Status.Should().Be("PendingPayment");
    }

    private CreateAppointmentCommandHandler CreateHandler() =>
        new(_context, _tenantService, new DateTimeProvider(),
            new StubEmailService(), new StubAsaasService(),
            new StubErrorLogService(),
            NullLogger<CreateAppointmentCommandHandler>.Instance);

    private class StubErrorLogService : Scheduly.Application.Common.Interfaces.IErrorLogService
    {
        public Task LogAsync(Scheduly.Domain.Entities.ErrorLog errorLog) => Task.CompletedTask;
    }

    private class FailingAsaasService : Scheduly.Application.Common.Interfaces.IAsaasService
    {
        public Task<Scheduly.Application.Common.Models.Asaas.AsaasCustomerResponse> CreateOrUpdateCustomerAsync(
            string apiKey, string name, string cpfCnpj, string email, string? phone,
            string externalReference, CancellationToken ct)
            => throw new Scheduly.Domain.Exceptions.DomainException("ASAAS_ERROR", "Asaas API error");

        public Task<Scheduly.Application.Common.Models.Asaas.AsaasPaymentResponse> CreatePaymentWithSplitAsync(
            string apiKey, string asaasCustomerId, int amountInCents,
            string description, string externalReference, CancellationToken ct)
            => throw new Scheduly.Domain.Exceptions.DomainException("ASAAS_ERROR", "Asaas API error");

        public Task<Scheduly.Application.Common.Models.Asaas.AsaasAccountResponse> ValidateApiKeyAsync(
            string apiKey, CancellationToken ct)
            => throw new Scheduly.Domain.Exceptions.DomainException("ASAAS_ERROR", "Asaas API error");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
