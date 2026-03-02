using Microsoft.Extensions.Logging;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Entities;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.Infrastructure.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(ApplicationDbContext dbContext, ILogger<ErrorLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogAsync(ErrorLog errorLog)
    {
        try
        {
            _dbContext.ErrorLogs.Add(errorLog);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist error log to database");
        }
    }
}
