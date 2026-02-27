using Hangfire;
using Microsoft.EntityFrameworkCore;
using Scheduly.Api.Middleware;
using Scheduly.Application;
using Scheduly.Infrastructure;
using Scheduly.Infrastructure.Jobs;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Override connection string from env var (Railway double-underscore may not bind automatically)
var dbConnEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbConnEnv))
    builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnEnv;

// Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Layers
builder.Services.AddApplication();
var isTesting = builder.Environment.IsEnvironment("Testing");
builder.Services.AddInfrastructure(builder.Configuration, useHangfire: !isTesting, useDatabase: !isTesting);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
var healthChecksBuilder = builder.Services.AddHealthChecks();
if (!isTesting)
    healthChecksBuilder.AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// CORS
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins?.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        else
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    }));

var app = builder.Build();

// Auto-migrate (all environments — Railway/production has no direct DB access)
if (!isTesting)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

// Hangfire
if (!isTesting)
{
    // Dashboard only in Development (no auth configured)
    if (app.Environment.IsDevelopment())
        app.UseHangfireDashboard("/hangfire");

    // Use service-based API instead of static RecurringJob (requires initialized storage)
    var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();

    jobManager.AddOrUpdate<AppointmentReminderJob>(
        "appointment-reminders",
        job => job.ExecuteAsync(),
        "*/15 * * * *"); // Every 15 minutes

    jobManager.AddOrUpdate<RefreshTokenCleanupJob>(
        "refresh-token-cleanup",
        job => job.ExecuteAsync(),
        Cron.Daily);
}

app.Run();

// Make Program accessible for integration tests
public partial class Program;
