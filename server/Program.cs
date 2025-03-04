using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using server;
using server.Properties;
using System.Text.Json;

Database database = new();
var db = database.Connection();
Queries queries = new(db);
Mail newmail = new Mail();

var builder = WebApplication.CreateBuilder(args);

// Lägg till CORS-tjänsten till DI-container
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5173") // Här anger du frontends URL (eller "*" om alla origin ska tillåtas)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// här konfigureras sessionshantering
builder.Services.AddDistributedMemoryCache();  // bra att veta att detta använder minnescache för sessioner
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); //om du är inaktiv så kommer du bli utloggad efter en vis tid har satt en minut för tester
    options.Cookie.HttpOnly = true; // Skyddar så att cookien inte kan nås via js
    options.Cookie.IsEssential = true; // Cookien krävs för att sessionen ska kunna fungera
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // cookien skickas bara via https
    options.Cookie.SameSite = SameSiteMode.Strict; // förhindrar att cookien skickas vid externa förfrågningar
});

// här konfiguerar vi autentisering med cookies 
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/api/login";  // om den som använder sidan inte är behörig så skickas den tillbaka till log in 
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
        options.Cookie.SameSite = SameSiteMode.Strict; 
    });

builder.Services.AddAuthorization(); // Detta aktiverar behörighetssystem

var app = builder.Build();

// Middleware-pipline vilket är en sekvens av middleware-komponenter som hanterar http-förfrågningar och svar
app.UseSession();  //aktiverar Sessionshanteringen
app.UseRouting(); // aktiverar routing för API-endpoints
app.UseAuthentication(); // Aktiverar autentisering
app.UseAuthorization();  // aktiverar behörighetshantering
app.UseMiddleware<AuthMiddleware>();  // Detta är min egna middleware för autentisering 

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/kontaktaoss/{company}", async (string company) =>
{
    var companyDetails = await queries.GetCompanyName(company);
    return companyDetails;
});

app.MapGet("/api/Chat/{chatId:int}", async (int chatId) =>
{
    var chatHistory = await queries.GetChatHistory(chatId);
    return chatHistory;
});

app.MapGet("/api/editCoWorker", async (HttpContext context) =>
{
    var employees = await queries.GetEmployees();
    return employees;
});

app.MapPost("/api/ChatResponse/{chatId}", async (HttpContext context) =>
{
    var chatId = int.Parse(context.Request.RouteValues["chatId"]?.ToString());
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var chatData = JsonSerializer.Deserialize<ChatData>(body);
    chatData.chatId = chatId;

    string emailConfirmationAdress = await queries.WriteChatToDB(chatData);
    if (emailConfirmationAdress != "")
    {
        await newmail.emailConfirmationOnAnswer(emailConfirmationAdress, chatData.chatId);
        Console.WriteLine("Email sent placeholder to email: " + emailConfirmationAdress);
    }
});

app.MapGet("/api/arbetarsida/{company}", async (string company) =>
{
    var chats = await queries.GetChatsForCsRep(company);
    return chats;
});

// API_endpoint för att kunna logga in
app.MapPost("/api/login", async (HttpContext context) =>
{
    try 
    {
        // Läser in förfrågningens JSON och deserialiserar den
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
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await context.SignInAsync(
                "CookieAuth",  
                new ClaimsPrincipal(new ClaimsIdentity(
                    new[] {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("IsAdmin", user.IsAdmin.ToString())
                    },
                    "CookieAuth")),
                authProperties
            );

            context.Session.SetString("UserId", user.Id.ToString());
            context.Session.SetString("UserEmail", user.Email);
            context.Session.SetString("IsAdmin", user.IsAdmin.ToString());

            return Results.Ok(new { 
                token = "test-token",
                user = new {
                    id = user.Id,
                    email = user.Email,
                    company = user.Company,
                    isCustomerServiceUser = user.IsCustomerServiceUser,
                    isAdmin = user.IsAdmin,
                    companyName = user.CompanyName
                }
            });
        }
        return Results.Unauthorized(); 
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        return Results.BadRequest(new { message = "An error occurred during login." });
    }
});

// API-endpoint för att se om användaren skulle ha en aktiv session
app.MapGet("/api/check-session", (HttpContext context) =>
{
    var userId = context.Session.GetString("UserId");
    var email = context.Session.GetString("UserEmail");
    var isAdmin = context.Session.GetString("IsAdmin");

    // felsökning
    Console.WriteLine($"Session Data: UserId={userId}, Email={email}, IsAdmin={isAdmin}");

    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        userId,
        email,
        isAdmin = bool.Parse(isAdmin ?? "false")
    });
});

app.MapPost("/api/form", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var ticketInformation = JsonSerializer.Deserialize<Ticket>(body);

        if (ticketInformation == null || string.IsNullOrEmpty(ticketInformation.email) ||
            string.IsNullOrEmpty(ticketInformation.option) || string.IsNullOrEmpty(ticketInformation.description))
        {
            return Results.BadRequest(new { message = "All fields have to be entered" });
        }

        if (ticketInformation != null)
        {
            await queries.CompanyName(ticketInformation);
            await queries.customerTempUser(ticketInformation);
            await queries.postNewTicket(ticketInformation);

            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
    }

    return Results.BadRequest();
});

app.Run();
Console.ReadLine();

// En klass för att kunna representera inloggningsförfrågningar
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class ChatData
{
    public string message { get; set; }
    public string email { get; set; }
    public int chatId { get; set; }
    public bool csrep { get; set; }
}
