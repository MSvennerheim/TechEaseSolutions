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

// här konfigureras sessionshantering
builder.Services.AddDistributedMemoryCache();  // bra att veta att detta använder minnescache för sessioner
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1); //om du är inaktiv så kommer du bli utloggad efter en vis tid har satt en minut för tester
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
    // Console.WriteLine(company);
    var companyDetails = await queries.GetCompanyName(company);
    
    // Console.WriteLine(companyDetails);
    return companyDetails;
});

app.MapGet("/api/Chat/{chatId:int}", async (int chatId, HttpContext context) =>
{
    
    // as long as your session is logged in to the correct company you can access the chat. 
    // if you're a customer query checks if you're the sender of a message in chat. 
    
    var user = new User
    {
        Email = context.Session.GetString("UserEmail"),
        CompanyName = context.Session.GetString("CompanyName"),
        CsRep = Convert.ToBoolean(context.Session.GetString("CsRep")),
        ChatId = chatId
    }; 
    
        var chatHistory = await queries.GetChatHistory(user);
        return chatHistory;

});

app.MapPost("/api/ChatResponse/", async (HttpContext context) =>
{
    
    var chatId = int.Parse(context.Request.RouteValues["chatId"]?.ToString());
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var chatData = JsonSerializer.Deserialize<ChatData>(body);
    
    chatData.chatId = chatId;
    chatData.email = context.Session.GetString("UserEmail");
    chatData.csrep = Convert.ToBoolean(context.Session.GetString("CsRep"));

    // deserialize json before putting into WriteChatToDB
    // (needed here since chatId is needed for emailConfirmationOnAnswer)

    string emailConfirmationAdress = await queries.WriteChatToDB(chatData);
    if (emailConfirmationAdress != "")
    {
        // hopefully this'll work, but first get in the response to db
        await newmail.emailConfirmationOnAnswer(emailConfirmationAdress, chatData.chatId);
        Console.WriteLine("Email sent placeholder to email: " + emailConfirmationAdress);
    }
    
});


app.MapGet("/api/arbetarsida/", async (HttpContext context) =>
{
    string company = context.Session.GetString("CompanyName");
    bool csRep = Convert.ToBoolean(context.Session.GetString("CsRep"));
    if (csRep)
    {
        var chats = await queries.GetChatsForCsRep(company);
        return chats;
    }
    return "no access";
});

app.MapPost("/api/guestLogin", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var loginData = JsonSerializer.Deserialize<LoginRequest>(body);

    try
    {
        
    if (loginData == null || string.IsNullOrEmpty(loginData.Email) || loginData.ChatId != null)
    {
        return Results.BadRequest(new { message = "Email and chat are required" });
    }

    string decodedEmail = Uri.UnescapeDataString(loginData.Email);
    var user = await queries.ValidateTempUser(decodedEmail, loginData.ChatId);
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
                        new Claim("IsAdmin", user.IsAdmin.ToString()), 
                        new Claim("CsRep", user.CsRep.ToString()) ,
                    },
                    "CookieAuth")),
                authProperties
            );
            context.Session.SetString("UserId", user.Id.ToString());
            context.Session.SetString("UserEmail", user.Email);
            context.Session.SetString("IsAdmin", user.IsAdmin.ToString());
            context.Session.SetString("CsRep", user.CsRep.ToString());
            context.Session.SetString("CompanyName", user.CompanyName);
            context.Session.SetInt32("ChatId", user.ChatId);

            
            
            // Retunerar inloggningsdata
            return Results.Ok(new { 
                token = "test-token",
                user = new {
                    id = user.Id,
                    email = user.Email,
                    company = user.Company,
                    companyName = user.CompanyName,
                    csRep = user.CsRep,
                    isAdmin = user.IsAdmin,
                    chatId = user.ChatId
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


// API_endpoint för att kunna logga in
app.MapPost("/api/login", async (HttpContext context) =>
{
    try 
    {
        // Läser in förfrågningens JSON och deserialiserar den
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var loginData = JsonSerializer.Deserialize<LoginRequest>(body);

        
        // Kontroll om en epost och lösenord har skickats 
        if (loginData == null || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
        {
            return Results.BadRequest(new { message = "Email and password are required" });
        }
        
        
        // checkar användaren genom att kolla up den i databasen
        var user = await queries.ValidateUser(loginData.Email, loginData.Password);
        if (user != null)
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // behåller sessionen även om den har stängts 
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24) // sessionen gäller i 24 timmar 
            };
            
            // Skapar en cookie-baserad autentisering för användaren. 
            await context.SignInAsync(
                "CookieAuth",  
                new ClaimsPrincipal(new ClaimsIdentity(
                    new[] {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  //sparar användarens Id
                        new Claim(ClaimTypes.Email, user.Email), // sparar användarens  e-post 
                        new Claim("IsAdmin", user.IsAdmin.ToString()), // sparar om användaren är admin eller inte
                        new Claim("CsRep", user.CsRep.ToString()) // sparar om användaren är admin eller inte

                    },
                    "CookieAuth")),
                authProperties
            );
             // lagrar användarensinformation i sessionen
            context.Session.SetString("UserId", user.Id.ToString());
            context.Session.SetString("UserEmail", user.Email);
            context.Session.SetString("IsAdmin", user.IsAdmin.ToString());
            context.Session.SetString("CsRep", user.CsRep.ToString());
            context.Session.SetString("CompanyName", user.CompanyName);

            
            
            // Retunerar inloggningsdata
            return Results.Ok(new { 
                token = "test-token",
                user = new {
                    id = user.Id,
                    email = user.Email,
                    company = user.Company,
                    companyName = user.CompanyName,
                    csRep = user.CsRep,
                    isAdmin = user.IsAdmin 
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

    // om ingen användare är inloggad ska det retunera unauthorized
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    // retunerar sessionens inforamation till frontend
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
        
        
        if (ticketInformation == null || string.IsNullOrEmpty(ticketInformation.email) || string.IsNullOrEmpty(ticketInformation.option) || string.IsNullOrEmpty(ticketInformation.description))
        {
            return Results.BadRequest(new { message = "All fields have to be entered" });
        }
        
        if (ticketInformation != null)
        {
            await queries.CompanyName(ticketInformation);
            //Creates a temp user for a ticket.
            await queries.customerTempUser(ticketInformation);
            await queries.postNewTicket(ticketInformation);
            //Creates a new confirmation mail that gets sent to the user in question.
            // newmail.generateNewIssue(ticketInformation);
            
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
    
    public int ChatId { get; set; }
}

public class ChatData
{
    public string message { get; set; }
    public string email { get; set; }
    public int chatId { get; set; }
    // public int CaseType { get; set; }
    // public string Company { get; set; }
    public bool csrep { get; set; }
}

