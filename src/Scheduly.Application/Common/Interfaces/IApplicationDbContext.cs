using Microsoft.EntityFrameworkCore;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Service> Services { get; }
    DbSet<Vacancy> Vacancies { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ErrorLog> ErrorLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
