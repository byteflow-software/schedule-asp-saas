using System.Net.Http.Headers;
using System.Text;

namespace Scheduly.Api.Middleware;

public class CoverageAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _username;
    private readonly string _password;

    public CoverageAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _username = configuration["Coverage:Username"] ?? "dev";
        _password = configuration["Coverage:Password"] ?? "dev";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/coverage"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            SetUnauthorized(context);
            return;
        }

        try
        {
            var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader!);
            if (authHeaderVal.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase)
                && authHeaderVal.Parameter is not null)
            {
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderVal.Parameter));
                var parts = credentials.Split(':', 2);

                if (parts.Length == 2 && parts[0] == _username && parts[1] == _password)
                {
                    await _next(context);
                    return;
                }
            }
        }
        catch
        {
            // Invalid auth header format
        }

        SetUnauthorized(context);
    }

    private static void SetUnauthorized(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Coverage Reports\"";
    }
}
