using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Vacancies.Commands.BulkCreateVacancies;
using Scheduly.Application.Features.Vacancies.Commands.CreateVacancy;
using Scheduly.Application.Features.Vacancies.Commands.DeleteVacancy;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Vacancies;

public class VacancyCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();
    private readonly Guid _userId;
    private readonly Guid _serviceId;

    public VacancyCommandHandlerTests()
    {
        _tenantService = new CurrentTenantService();
        _tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);

        _userId = Guid.NewGuid();
        _serviceId = Guid.NewGuid();

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
            Id = _userId, TenantId = _tenantId, FullName = "Dr. Smith",
            Email = "smith@clinic.com", PasswordHash = "hash",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = _serviceId, TenantId = _tenantId, Name = "Consultation",
            DurationMinutes = 30, PriceInCents = 5000,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    // ── CreateVacancy ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateVacancy_ValidCommand_CreatesWithIsBookedFalse()
    {
        var handler = new CreateVacancyCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new CreateVacancyCommand(
            _userId, _serviceId,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsBooked.Should().BeFalse();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateVacancy_SetsCorrectTenantId()
    {
        var handler = new CreateVacancyCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new CreateVacancyCommand(
            _userId, _serviceId,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var result = await handler.Handle(command, CancellationToken.None);

        var saved = await _context.Vacancies.FirstAsync(v => v.Id == result.Id);
        saved.TenantId.Should().Be(_tenantId);
    }

    // ── BulkCreateVacancies ────────────────────────────────────────────────

    [Fact]
    public async Task BulkCreateVacancies_MultipleSlots_CreatesAll()
    {
        var handler = new BulkCreateVacanciesCommandHandler(_context, _tenantService, _dateTimeProvider);
        var baseTime = DateTime.UtcNow.AddDays(1);
        var slots = new List<SlotDto>
        {
            new(baseTime, baseTime.AddMinutes(30)),
            new(baseTime.AddMinutes(30), baseTime.AddHours(1)),
            new(baseTime.AddHours(1), baseTime.AddMinutes(90))
        };

        var command = new BulkCreateVacanciesCommand(_userId, _serviceId, slots);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().AllSatisfy(v => v.IsBooked.Should().BeFalse());
    }

    [Fact]
    public async Task BulkCreateVacancies_EmptySlots_ReturnsEmptyList()
    {
        var handler = new BulkCreateVacanciesCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new BulkCreateVacanciesCommand(_userId, _serviceId, new List<SlotDto>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeEmpty();
    }

    // ── DeleteVacancy ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteVacancy_UnbookedVacancy_RemovesFromDb()
    {
        // Arrange: create a vacancy first
        var createHandler = new CreateVacancyCommandHandler(_context, _tenantService, _dateTimeProvider);
        var created = await createHandler.Handle(
            new CreateVacancyCommand(_userId, _serviceId,
                DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)),
            CancellationToken.None);

        var countBefore = await _context.Vacancies.CountAsync();

        // Act
        var deleteHandler = new DeleteVacancyCommandHandler(_context);
        await deleteHandler.Handle(new DeleteVacancyCommand(created.Id), CancellationToken.None);

        // Assert
        var countAfter = await _context.Vacancies.CountAsync();
        countAfter.Should().Be(countBefore - 1);
    }

    [Fact]
    public async Task DeleteVacancy_BookedVacancy_ThrowsDomainException()
    {
        // Arrange: create a booked vacancy directly
        var vacancyId = Guid.NewGuid();
        _context.Vacancies.Add(new Vacancy
        {
            Id = vacancyId, TenantId = _tenantId, UserId = _userId,
            ServiceId = _serviceId, StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2), IsBooked = true,
            AppointmentId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new DeleteVacancyCommandHandler(_context);

        // Act
        var act = () => handler.Handle(new DeleteVacancyCommand(vacancyId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*booked*");
    }

    [Fact]
    public async Task DeleteVacancy_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new DeleteVacancyCommandHandler(_context);

        var act = () => handler.Handle(new DeleteVacancyCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
