using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Infrastructure.Jobs;

public class RefreshTokenCleanupJob
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<RefreshTokenCleanupJob> _logger;

    public RefreshTokenCleanupJob(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ILogger<RefreshTokenCleanupJob> logger)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var cutoff = _dateTimeProvider.UtcNow.AddDays(-7);

        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoff || (rt.RevokedAt != null && rt.RevokedAt < cutoff))
            .ToListAsync();

        if (expiredTokens.Count == 0) return;

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", expiredTokens.Count);
    }
}
