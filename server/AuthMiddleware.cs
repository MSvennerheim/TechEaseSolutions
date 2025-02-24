using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Npgsql;
using System.Text.Json;

namespace server;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NpgsqlDataSource _dataSource;
    private readonly HashSet<string> _allowedReferrers = new()
    {
        "mail.google.com",
        "outlook.live.com",
        "outlook.office365.com",
        "outlook.office.com",
        "yahoo.com",
        "mail.yahoo.com",
        "localhost:5173"
    };

    public AuthMiddleware(RequestDelegate next, NpgsqlDataSource dataSource)
    {
        _next = next;
        _dataSource = dataSource;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        Console.WriteLine($"Request Path: {path}");

        if (path.StartsWith("/case/"))
        {
            var token = context.Request.RouteValues["token"]?.ToString();
            var email = context.Request.Query["email"].ToString();
            var referer = context.Request.Headers.Referer.ToString();

            Console.WriteLine($"Middleware: Token={token}, Email={email}, Referer={referer}");

            
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                await SendUnauthorizedResponse(context, "Token and email are required.");
                return;
            }

            
            var sessionEmail = context.Session.GetString("EmailSession");
            var sessionToken = context.Session.GetString("TokenSession");

            if (sessionEmail == email && sessionToken == token)
            {
                Console.WriteLine("Valid session found, allowing access.");
                await _next(context);
                return;
            }

            
            var queries = context.RequestServices.GetRequiredService<Queries>();
            bool isValid = await queries.ValidateTokenAndEmail(token, email, context);

            if (!isValid)
            {
                await SendUnauthorizedResponse(context, "Invalid or expired token.");
                return;
            }

            
            context.Session.SetString("EmailSession", email);
            context.Session.SetString("TokenSession", token);
            Console.WriteLine($"Session set for email: {email} and token: {token}");

            await _next(context);
            return;
        }

        await HandleProtectedRoutes(context);
    }
    private async Task HandleProtectedRoutes(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        var protectedRoutes = new[] { "/admin", "/arbetarsida" };

        if (protectedRoutes.Any(route => path.StartsWith(route)))
        {
            var userId = context.Session.GetString("UserId");
            var isAdmin = context.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(userId))
            {
                await SendUnauthorizedResponse(context, "Please log in to access this resource.");
                return;
            }

            if (path.StartsWith("/admin") && isAdmin != "True")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Access denied. Admin privileges required." });
                return;
            }
        }

        await _next(context);
    }

    private static async Task SendUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message });
    }
}