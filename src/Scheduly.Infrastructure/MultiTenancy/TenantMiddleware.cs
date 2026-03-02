using Microsoft.AspNetCore.Http;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Infrastructure.MultiTenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> ExemptPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/register",
        "/api/auth/login",
        "/api/auth/refresh-token",
        "/api/auth/revoke-token",
        "/health",
        "/health/ready",
        "/health/live",
        "/swagger",
        "/api/error-logs"
    };

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip tenant resolution for exempt paths
        if (IsExemptPath(path))
        {
            await _next(context);
            return;
        }

        // Try to get tenant from header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
            Guid.TryParse(tenantHeader.ToString(), out var tenantId))
        {
            // If user is authenticated, validate tenant matches JWT claim
            var claimTenantId = context.User?.FindFirst("tenant_id")?.Value;
            if (claimTenantId is not null && claimTenantId != tenantId.ToString())
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant mismatch with authenticated user." });
                return;
            }

            tenantService.SetTenantId(tenantId);
            await _next(context);
            return;
        }

        // Try to get tenant from JWT claim
        var jwtTenantId = context.User?.FindFirst("tenant_id")?.Value;
        if (jwtTenantId is not null && Guid.TryParse(jwtTenantId, out var parsedTenantId))
        {
            tenantService.SetTenantId(parsedTenantId);
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id header is required." });
    }

    private static bool IsExemptPath(string path)
    {
        foreach (var exempt in ExemptPaths)
        {
            if (path.StartsWith(exempt, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
