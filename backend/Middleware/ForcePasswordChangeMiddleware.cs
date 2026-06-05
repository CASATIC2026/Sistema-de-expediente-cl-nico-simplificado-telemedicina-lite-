using System.Security.Claims;
using TelMedAPI.Helpers;

public class ForcePasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    public ForcePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            var mustChange = user.FindFirst("MustChangePassword")?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value?.ToLower();

            if (mustChange == "true" && role == "doctor")
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";

                var allowedPaths = new[]
                {
                    "/api/auth/cambiar-password",
                    "/api/auth/login"
                };

                if (!allowedPaths.Any(p => path.Contains(p)))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync(
                        "Debe cambiar su contraseña antes de acceder al sistema."
                    );
                    return;
                }
            }
        }
        await _next(context);
    }
}