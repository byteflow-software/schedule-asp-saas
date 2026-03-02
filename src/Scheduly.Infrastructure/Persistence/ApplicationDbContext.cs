using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    public Guid CurrentTenantId => _currentTenantService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for multi-tenancy
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Service>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Vacancy>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Transaction>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

        // Customer has both tenant scope and soft delete
        modelBuilder.Entity<Customer>().HasQueryFilter(e =>
            e.TenantId == CurrentTenantId && !e.IsDeleted);
    }
}
