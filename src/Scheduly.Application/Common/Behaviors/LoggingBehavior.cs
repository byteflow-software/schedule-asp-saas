using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService _currentTenantService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService,
        ICurrentTenantService currentTenantService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _currentTenantService = currentTenantService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var tenantId = _currentTenantService.TenantId;

        _logger.LogInformation("Handling {RequestName} | User: {UserId} | Tenant: {TenantId}",
            requestName, userId, tenantId);

        var sw = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        sw.Stop();

        _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms",
            requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
