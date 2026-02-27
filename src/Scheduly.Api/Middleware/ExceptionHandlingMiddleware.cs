using System.Text.Json;
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

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private record ErrorResponse(
        string Code,
        string Message,
        IDictionary<string, string[]>? Errors = null);
}
