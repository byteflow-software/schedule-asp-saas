using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Infrastructure.Identity;
using Scheduly.Infrastructure.Jobs;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Persistence.Interceptors;
using Scheduly.Infrastructure.Services;

namespace Scheduly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool useHangfire = true, bool useDatabase = true)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? (useDatabase ? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.") : null);

        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();

        // EF Core + PostgreSQL (skipped in test environments where database is replaced)
        if (useDatabase)
        {
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
                options.UseNpgsql(connectionString!)
                       .AddInterceptors(interceptor);
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        }

        // Multi-tenancy
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        // Identity
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();

        // Services
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<INotificationService, MockNotificationService>();
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Jobs
        services.AddScoped<AppointmentReminderJob>();
        services.AddScoped<RefreshTokenCleanupJob>();

        // JWT Settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? new JwtSettings();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Admin", "Staff"));

        // Hangfire
        if (useHangfire)
        {
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(opts =>
                    opts.UseNpgsqlConnection(connectionString)));
            services.AddHangfireServer();
        }

        services.AddHttpContextAccessor();

        return services;
    }
}
