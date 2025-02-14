namespace server;

public class AuthMiddleware  // behövde skapa en middleware för att kunna autentisera och hantera loggning och sessionhantering
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower(); 
        // skyddade routes Kom ihåg att routsen som ska bli skyddad måste in här.
        var protectedRoutes = new List<string>
        {
            "/admin",
            "/arbetarsida"
        };
    
        bool isProtectedRoute = protectedRoutes.Any(route => path.StartsWith(route, StringComparison.OrdinalIgnoreCase));

        if (isProtectedRoute)
        {
            var userId = context.Session.GetString("UserId");
            var isAdmin = context.Session.GetString("IsAdmin");
            
            
               // om user id är noll så kommer du inte in
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401; 
                await context.Response.WriteAsync("Unauthorized: Please log in.");
                return;
            }

            // Bara admin kan ta sig till admin sidan
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