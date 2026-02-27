using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(ApplicationDbContext) ||
                    d.ServiceType == typeof(IApplicationDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            // Build an isolated EF Core internal service provider for InMemory.
            // This prevents the "multiple database providers" error caused by
            // Npgsql services already registered in the main DI container
            // (Program.cs runs before ConfigureWebHost can set the environment).
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_dbName)
                       .UseInternalServiceProvider(efServiceProvider);
            });

            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            // Remove Hangfire hosted service registrations
            var hangfireDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName?.Contains("Hangfire") == true ||
                    d.ImplementationType?.FullName?.Contains("Hangfire") == true)
                .ToList();
            foreach (var d in hangfireDescriptors)
                services.Remove(d);
        });
    }
}
