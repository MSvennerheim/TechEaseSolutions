using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using server;
using server.Properties;
using System.Text.Json;
using Npgsql;

Database database = new();
var db = database.Connection();
Queries queries = new(db);
Mail newmail = new Mail();

var builder = WebApplication.CreateBuilder(args);
var connection = queries.GetDbConnection();
Console.WriteLine("Database connection retrived successfully");




// Service konfigurering
ConfigureServices(builder);

var app = builder.Build();

// Middleware konfigurering
ConfigureMiddleware(app);

// Route konfigurering
ConfigureRoutes(app);

app.Run();
Console.ReadLine();

// Service konfigurerings Metod
// Service konfigurerings Metod
void ConfigureServices(WebApplicationBuilder builder)
{
    
    var database = new Database();
    var dataSource = database.Connection();
// In Program.cs or Startup.cs
    
    //
    builder.Services.AddSingleton<NpgsqlDataSource>(_ => dataSource);

    //Fick lägga till cors för att själva den som öppnar sitt ärende från mail ska kunna see och skriva tillbaka förhoppingsvis kan jag hitta ett annat sätt senare
    builder.Services.AddCors();

    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();  // Viktigt om cookies används!
        });
    });


    // Behövde lägga till addScoped<T>() eftersom att Queries hanterar databasförfrågningarna. 
    // Vad gör den? Om en användare gör en förfrågan så skapas en instans av queries och används under hela förfrågningen och sedan när frågan är klar så förstörs objeketet automatiskt
    builder.Services.AddScoped<Queries>();
    
    // en enda instans av npgsqlDatasource skapas vid app start och återanvänds under hela applikationen
    // här säger den åt ASP.net core att alltid använda samma instans av datassource när en tjänst behöver npgsqlDatasource
    builder.Services.AddSingleton<NpgsqlDataSource>(_ => dataSource);
    // här konfigureras sessionshantering
    builder.Services.AddDistributedMemoryCache();  // bra att veta att detta använder minnescache för sessioner
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(60); //om du är inaktiv så kommer du bli utloggad efter en vis tid.
        options.Cookie.HttpOnly = true; // Skyddar så att cookien inte kan nås via js
        options.Cookie.IsEssential = true; // Cookien krävs för att sessionen ska kunna fungera
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // cookien skickas bara via http
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
}

// Middleware konfigurerings Metod
void ConfigureMiddleware(WebApplication app)
{
    // Middleware-pipline vilket är en sekvens av middleware-komponenter som hanterar http-förfrågningar och svar
    app.UseCors("AllowFrontend");
    app.UseSession();  //aktiverar Sessionshanteringen
    app.UseRouting(); // aktiverar routing för API-endpoints
    app.UseAuthentication(); // Aktiverar autentisering
    app.UseAuthorization();  // aktiverar behörighetshantering
    app.UseMiddleware<AuthMiddleware>();  // Detta är min egna middleware för autentisering 
}

// Route konfigurerings Method
void ConfigureRoutes(WebApplication app)
{
    app.MapGet("/", () => "Hello World!");

    // för att konfigurera case Routes
    ConfigureCaseRoutes(app);

    // Autensierings Routes
    ConfigureAuthRoutes(app);

    // chat routes
    ConfigureChatRoutes(app);
}

// Case Management Route konfiguering.
// Behövde skapa routes för annars förstår inte asp.net core hur den ska hantera parametrarna som skickas till en endpoint utan man måste tala om exakt hur det ska hanteras
// alltså "[FromRoute]" talar om att token kommer från en URL och "[FromServices]" talar om att queries ska injiceras via dependency injection-systemet. svårt att förstå I KNOWWWW men detta behövdes 
void ConfigureCaseRoutes(WebApplication app) {
    app.MapGet("/case/{token}", async ([FromRoute] string token, [FromQuery] string email, [FromServices] Queries queries, [FromServices] NpgsqlDataSource db, HttpContext context) =>
    {
        Console.WriteLine($"fetching case for token: {token}, email: {email}");

        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("email is required.");
            return Results.BadRequest( new {message = "Email is required."});
        }
        
        bool isValid = await queries.ValidateTokenAndEmail(token, email, context);
        Console.WriteLine($"Token validation result: {isValid}");

        if (!isValid)
        {
            Console.WriteLine("Invalid token or email.");
            return Results.Json( new { message = "invalid token or email."}, statusCode: StatusCodes.Status401Unauthorized );
        }

        const string sql = @" SELECT m.id, m.message, m.company, m.casetype, m.sender, m.timestamp
         FROM messages m JOIN case_tokens ct on m.chatid = ct.case_id
         WHERE ct.token = @token AND ct.email = @email LIMIT 1;";

        await using (var cmd = db.CreateCommand(sql))
        {
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@email", email);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

                if (await reader.ReadAsync())
                {
                    var caseData = new
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        Company = reader.GetInt32(2),
                        Casetype = reader.GetInt32(3),
                        Sender = reader.GetInt32(4),
                        Timestamp = reader.GetDateTime(5)
                    };
                    Console.WriteLine($"Case data was fetched successfully");
                    return Results.Ok(caseData);


                }
            }
        }
        Console.WriteLine("Case data fetched successfully.");
        return Results.NotFound("Case not found.");
    });

    app.MapPost("/api/create-case", async (HttpContext context) =>
    {
        try 
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var ticket = JsonSerializer.Deserialize<Ticket>(body);

            if (ticket == null || string.IsNullOrEmpty(ticket.email) || 
                string.IsNullOrEmpty(ticket.description))
            {
                return Results.BadRequest("Invalid ticket data.");
            }

            // lägg in caset och få ett chatt id genom att använda existerande queries
            await queries.CompanyName(ticket);
            await queries.customerTempUser(ticket);
            await queries.postNewTicket(ticket);

            // Generera token genom att använda redan existerande metod
            var token = await queries.GenerateAndStoreToken(ticket.chatid, ticket.email);

            // skicka token länken
            var mail = new Mail();
            await mail.SendTokenLink(ticket.email, ticket.chatid, token);

            return Results.Ok(new { chatId = ticket.chatid, token });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating case: {ex}");
            return Results.StatusCode(500);
        }
    });

    app.MapPost("/api/form", async (HttpContext context) =>
    {
        try
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var ticketInformation = JsonSerializer.Deserialize<Ticket>(body);
            
            if (ticketInformation == null || string.IsNullOrEmpty(ticketInformation.email) || 
                string.IsNullOrEmpty(ticketInformation.option) || 
                string.IsNullOrEmpty(ticketInformation.description))
            {
                return Results.BadRequest(new { message = "All fields have to be entered" });
            }
            
            if (ticketInformation != null)
            {
                // Populera ticket klassen längre fram och pusha den till databasen
                await queries.customerTempUser(ticketInformation);
                await queries.CompanyName(ticketInformation);
                await queries.postNewTicket(ticketInformation);

                // Generera token 
                var token = await queries.GenerateAndStoreToken(ticketInformation.chatid, ticketInformation.email);

                // skicka token länken
                var mail = new Mail();
                await mail.SendTokenLink(ticketInformation.email, ticketInformation.chatid, token);
                
                return Results.Ok();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
        return Results.BadRequest();
    });
}

// Autentisering Route Konfiguration
void ConfigureAuthRoutes(WebApplication app)
{
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
            if (loginData == null || string.IsNullOrEmpty(loginData.Email) || 
                string.IsNullOrEmpty(loginData.Password))
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
                            new Claim("IsAdmin", user.IsAdmin.ToString()) // sparar om användaren är admin eller inte 
                        },
                        "CookieAuth")),
                    authProperties
                );

                // lagrar användarensinformation i sessionen
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
}

// Chat Route Konfigurering
void ConfigureChatRoutes(WebApplication app)
{
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

    app.MapGet("/api/ChatResponse/{chatResponse}", async (string chatResponse) =>
    {
        // deserialize json before putting into WriteChatToDB
        // (needed here since chatId is needed for emailConfirmationOnAnswer)
        ChatData? chatData = JsonSerializer.Deserialize<ChatData>(chatResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        string emaiConfirmationAdress = await queries.WriteChatToDB(
            chatData.Message, 
            chatData.Email,  
            chatData.ChatId, 
            chatData.CaseType, 
            chatData.Company, 
            chatData.Csrep
        );
        
        if (emaiConfirmationAdress != "")
        {
            await newmail.emailConfirmationOnAnswer(emaiConfirmationAdress, chatData.ChatId);
        }
    });

    app.MapGet("/api/arbetarsida/{company}", async (string company) =>
    {
        try
        {
            var chatsJson = await queries.GetChatsForCsRep(company);
            var chats = JsonSerializer.Deserialize<List<object>>(chatsJson);
            return Results.Ok(chats);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching chats for company {company}: {ex}");
            return Results.StatusCode(500);
        }
    });
}

// Klasser 
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class ChatData
{
    public string Message { get; set; }
    public string Email { get; set; }
    public int ChatId { get; set; }
    public int CaseType { get; set; }
    public string Company { get; set; }
    public bool Csrep { get; set; }
}