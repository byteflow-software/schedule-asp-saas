using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.Api.Controllers;

[ApiController]
[Route("api/error-logs")]
[Authorize(Policy = "AdminOnly")]
public class ErrorLogsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public ErrorLogsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetErrorLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? level = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.ErrorLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(level))
            query = query.Where(e => e.Level == level);
        if (tenantId.HasValue)
            query = query.Where(e => e.TenantId == tenantId);
        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new { totalCount, page, pageSize, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetErrorLog(Guid id, CancellationToken ct)
    {
        var log = await _dbContext.ErrorLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        return log is null ? NotFound() : Ok(log);
    }
}
