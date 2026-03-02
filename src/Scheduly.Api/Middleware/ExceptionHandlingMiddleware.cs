using System.Text.Json;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using ValidationException = Scheduly.Application.Common.Exceptions.ValidationException;
using ForbiddenAccessException = Scheduly.Application.Common.Exceptions.ForbiddenAccessException;

namespace Scheduly.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse("VALIDATION_ERROR", "One or more validation errors occurred.", validationEx.Errors)),

            EntityNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ErrorResponse(notFoundEx.Code, notFoundEx.Message)),

            AppointmentOverlapException overlapEx => (
                StatusCodes.Status409Conflict,
                new ErrorResponse(overlapEx.Code, overlapEx.Message)),

            ForbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                new ErrorResponse("FORBIDDEN", "You do not have permission to perform this action.")),

            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse(domainEx.Code, domainEx.Message)),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("INTERNAL_ERROR",
                    _env.IsProduction()
                        ? "An unexpected error occurred."
                        : $"{exception.GetType().Name}: {exception.Message}"))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        await PersistErrorLogAsync(context, exception, statusCode);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private async Task PersistErrorLogAsync(HttpContext context, Exception exception, int statusCode)
    {
        try
        {
            var errorLogService = context.RequestServices.GetService<IErrorLogService>();
            if (errorLogService is null) return;

            var requestBody = SanitizeBody(await ReadRequestBodyAsync(context));

            var tenantIdClaim = context.User?.FindFirst("tenant_id")?.Value;
            var userIdClaim = context.User?.FindFirst("sub")?.Value;

            var errorLog = new ErrorLog
            {
                Level = statusCode >= 500 ? "Error" : "Warning",
                Message = Truncate(exception.Message, 2000),
                ExceptionType = exception.GetType().FullName,
                StackTrace = Truncate(exception.StackTrace, 8000),
                Source = exception.TargetSite?.DeclaringType?.Name ?? "Unknown",
                RequestPath = Truncate(context.Request.Path, 500),
                RequestMethod = context.Request.Method,
                RequestBody = Truncate(requestBody, 4000),
                HttpStatusCode = statusCode,
                TenantId = Guid.TryParse(tenantIdClaim, out var tid) ? tid : null,
                UserId = Guid.TryParse(userIdClaim, out var uid) ? uid : null,
            };

            if (exception.Data.Contains("AsaasRequestUrl"))
            {
                errorLog.ExternalRequestUrl = Truncate(exception.Data["AsaasRequestUrl"] as string, 1000);
                errorLog.ExternalResponseBody = Truncate(exception.Data["AsaasResponseBody"] as string, 4000);
                errorLog.ExternalStatusCode = exception.Data["AsaasStatusCode"] as int?;
            }

            await errorLogService.LogAsync(errorLog);
        }
        catch
        {
            // Must never throw — the error response must still be returned
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpContext context)
    {
        try
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            return string.IsNullOrWhiteSpace(body) ? null : body;
        }
        catch
        {
            return null;
        }
    }

    private static string? SanitizeBody(string? body)
    {
        if (body is null) return null;
        return System.Text.RegularExpressions.Regex.Replace(
            body, @"""(password|senha|secret|token|apiKey|api_key)"":\s*""[^""]*""",
            @"""$1"":""[REDACTED]""", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string? Truncate(string? value, int maxLength)
        => value is null ? null : value.Length <= maxLength ? value : value[..maxLength];

    private record ErrorResponse(
        string Code,
        string Message,
        IDictionary<string, string[]>? Errors = null);
}
