using server;
using server.Properties;
using System.Text.Json;
Database database = new();
var db = database.Connection();
Queries queries = new(db);

var builder = WebApplication.CreateBuilder(args);




var app = builder.Build();


app.MapGet("/", () => "Hello World!");

// Lägger till en login endpoint
app.MapPost("/login", async (HttpContext context) =>
{
    try 
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var loginData = JsonSerializer.Deserialize<LoginRequest>(body);

        if (loginData == null || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
        {
            return Results.BadRequest(new { message = "Email and password are required" });
        }

        var user = await queries.ValidateUser(loginData.Email, loginData.Password);
        if (user != null)
        {
            return Results.Ok(new { 
                token = "test-token",
                user = new {
                    id = user.Id,
                    email = user.Email,
                    company = user.Company,
                    isCustomerServiceUser = user.IsCustomerServiceUser,
                    isAdmin = user.IsAdmin
                }
            });
        }
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/form", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var ticketInformation = JsonSerializer.Deserialize<Ticket>(body);
        
        
        if (ticketInformation == null || string.IsNullOrEmpty(ticketInformation.email) || string.IsNullOrEmpty(ticketInformation.option) || string.IsNullOrEmpty(ticketInformation.description))
        {
            return Results.BadRequest(new { message = "All fields have to be entered" });
        }

        
        if (ticketInformation != null)
        {
            queries.customerTempUser(ticketInformation.email);
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
    }
    
    return Results.BadRequest();
});







Mail newmail = new Mail();
//newmail.generateNewIssue();

app.Run();
Console.ReadLine();

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
