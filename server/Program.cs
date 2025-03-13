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
    options.IdleTimeout = TimeSpan.FromMinutes(120); //om du är inaktiv så kommer du bli utloggad efter en vis tid har satt en minut för tester
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
    var user = new User
    {
        Email = context.Session.GetString("UserEmail"),
        CompanyName = context.Session.GetString("CompanyName"),
        CsRep = Convert.ToBoolean(context.Session.GetString("CsRep")),
        ChatId = Convert.ToInt32(context.Session.GetInt32("ChatId"))
    };

    Console.WriteLine($"API Call - Email: {user.Email}, Company: {user.CompanyName}, ChatId: {chatId}, CsRep: {user.CsRep}");

    if (chatId == user.ChatId && !user.CsRep)
    {
        Console.WriteLine("Customer accessing their own chat");
        var chatHistory = await queries.GetChatHistory(user);
        return Results.Content(chatHistory, "application/json");
    } 
    if (user.CsRep)
    {
        Console.WriteLine("CS Rep accessing chat");
        user.ChatId = chatId;
        var chatHistory = await queries.GetChatHistory(user);
        return Results.Content(chatHistory, "application/json");
    } 
    
    Console.WriteLine("Access denied - neither customer's own chat nor CS Rep");
    return Results.NotFound("no chat found");
});

app.MapPost("/api/ChatResponse/{chatId}", async (HttpContext context) =>
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

app.MapGet("/api/GetCoWorker", async (HttpContext context) =>
{
    var company = context.Session.GetString("CompanyName");
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));
    Console.WriteLine("Company name:" + company + "isAdmin: " + isAdmin);
    if (isAdmin)
    {
        var employees = await queries.GetEmployees(company);
        return employees;
    }
    return null;
});

app.MapPost("/api/arbetarsida/", async (HttpContext context) =>
{
    string company = context.Session.GetString("CompanyName");
    bool csRep = Convert.ToBoolean(context.Session.GetString("CsRep"));
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var sortingObject = JsonSerializer.Deserialize<ChatSortingObject>(body);
    
    if (csRep)
    {
        var chats = await queries.GetChatsForCsRep(company, sortingObject.getAllChats);
        return chats;
    }
    return "no access";
});

app.MapGet("/api/arbetarsida", async (HttpContext context) =>
{
    var user = new User
    {
        Email = context.Session.GetString("UserEmail"),
        CompanyName = context.Session.GetString("CompanyName"),
        CsRep = Convert.ToBoolean(context.Session.GetString("CsRep"))
    };

    if (user.CsRep)
    {
        var chats = await queries.GetChatsForCsRep(user.CompanyName, true);
        return Results.Ok(chats);
    }

    return Results.Forbid();
});

app.MapPost("/api/guestLogin", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var loginData = JsonSerializer.Deserialize<LoginRequest>(body);

        if (loginData == null || string.IsNullOrEmpty(loginData.Email) || loginData.ChatId is null)
        {
            return Results.BadRequest(new { message = "Email and chat are required" });
        }

        string decodedEmail = Uri.UnescapeDataString(loginData.Email);
        Console.WriteLine($"Guest login attempt - Email: {decodedEmail}, ChatId: {loginData.ChatId}");
        
        var user = await queries.ValidateTempUser(decodedEmail, Convert.ToInt32(loginData.ChatId));
        if (user != null)
        {
            // Sätt alla nödvändiga sessionsvariabler
            context.Session.SetString("UserId", user.Id.ToString());
            context.Session.SetString("UserEmail", user.Email);
            context.Session.SetString("IsAdmin", "false");  // Gäster är aldrig admins
            context.Session.SetString("CsRep", "false");    // Gäster är aldrig kundtjänst
            context.Session.SetString("CompanyName", user.CompanyName);  // Viktigt!
            context.Session.SetInt32("ChatId", user.ChatId);            // Viktigt!

            Console.WriteLine($"Guest login successful - Setting session data:");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"CompanyName: {user.CompanyName}");
            Console.WriteLine($"ChatId: {user.ChatId}");

            return Results.Ok(new { 
                user = new {
                    id = user.Id,
                    email = user.Email,
                    companyName = user.CompanyName,
                    chatId = user.ChatId,
                    csRep = false,
                    isAdmin = false
                }
            });
        }

        Console.WriteLine("Guest login failed - User not found or invalid");
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Guest login error: {ex}");
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
            context.Session.SetInt32("company", user.Company);
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

        if (ticketInformation == null || string.IsNullOrEmpty(ticketInformation.email) ||
            string.IsNullOrEmpty(ticketInformation.option) || string.IsNullOrEmpty(ticketInformation.description))
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

app.MapPost("/api/NewCustomerSupport", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var user = JsonSerializer.Deserialize<User>(body);
    
    Console.WriteLine("Program.cs");
    int? Company = context.Session.GetInt32("company");
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));
    

    Console.WriteLine($"Company ID: {Company}, isAdmin: {isAdmin}");

    if (Company == null)
    {
        return Results.BadRequest("Company ID is required.");
    }

    if (string.IsNullOrWhiteSpace(user.Email))
    {
        return Results.BadRequest("Email is required.");
    }

    if (isAdmin)
    {
        await queries.PostNewCsRep(Company.Value, user.Email);
        return Results.Ok("Customer support rep added successfully.");
    }

    return Results.Forbid();
});

app.MapPost("api/deleteCsRep", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var user = JsonSerializer.Deserialize<User>(body);
    
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));
    int Company = Convert.ToInt32(context.Session.GetInt32("company"));

    // check so that only a session that's admin and for the same company can delete account
    
    if (isAdmin)
    {
        await queries.RemoveCsRep(Company, user.Email);
        return Results.Ok("Customer support rep deleted.");
    }

    return Results.BadRequest("You do not have access to this function.");

});


// Reset password api
app.MapPost("/api/reset-password", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var resetData = JsonSerializer.Deserialize<PasswordResetRequest>(body);
        
        if (resetData == null || string.IsNullOrEmpty(resetData.email) || 
            string.IsNullOrEmpty(resetData.token) || string.IsNullOrEmpty(resetData.newPassword))
        {
            return Results.BadRequest(new { message = "Email, token and new password are required" });
        }
        
        bool resetSuccessful = await queries.ValidateTokenAndResetPassword(
            resetData.email, resetData.token, resetData.newPassword);
        
        if (resetSuccessful)
        {
            return Results.Ok(new { message = "Password reset successful" });
        }
        
        return Results.BadRequest(new { message = "Invalid or expired token" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        return Results.BadRequest(new { message = "An error occurred during password reset." });
    }
});

app.MapGet("/api/companies", async (HttpContext context) => {
    try 
    {
        var companies = await queries.GetCompanies();
        Console.WriteLine($"Fetched {companies.Count} companies");
        return Results.Json(companies);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching companies: {ex}");
        return Results.StatusCode(500);
    }
});

app.Run();
Console.ReadLine();


// En klass för att kunna representera inloggningsförfrågningar
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ChatId { get; set; }
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
public class NewCsRepRequest
{
    public string Email {get; set;}
    public string CompanyName { get; set;}
    public bool IsAdmin { get; set;}
}

// klass för passwordreset
public class PasswordResetRequest
{
    public string email { get; set; }
    public string token { get; set; }
    public string newPassword { get; set; }
}


public class ChatSortingObject
{
    public bool getAllChats { get; set; }
}
