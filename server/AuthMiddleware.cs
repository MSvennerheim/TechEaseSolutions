namespace server;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Npgsql;
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NpgsqlDataSource _dataSource;

    
    //Detta ör min custom autentiseringsmiddleware som används för att skydda api routes i vår applikation.
    //Middlewaren ser också till så att sessionen innehåller en giltig användar id innan den tillåter åtkomst.
    public AuthMiddleware(RequestDelegate next, NpgsqlDataSource dataSource)
    {
        _next = next;
        _dataSource = dataSource;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        // Detta är en lista över vilka routes som kommer kräva autentisering eller en specefik roll.
        // Så vi måste lägga till mer här sen
        var protectedRoutes = new List<string>
        {
            "/admin",
            "/arbetarsida"
        };

        bool isProtectedRoute = protectedRoutes.Any(route => 
            path.StartsWith(route, StringComparison.OrdinalIgnoreCase));

        if (isProtectedRoute)
        {
            
            // denna kontrollerar om användaren är inloggad och middleware hämtar användarens userid från sessionen
            // och om det saknas ett userid så är användaren inte inloggad och då blir den UnAuthorized
            var userId = context.Session.GetString("UserId");
            var isAdmin = context.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Please log in.");
                return;
            }
                   //Den kontrollerar om användaren är admin för att få åtkomst till /admin
            if (path.StartsWith("/admin") && isAdmin != "True")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: You do not have access.");
                return;
            }
        }

        await _next(context);
    }
}