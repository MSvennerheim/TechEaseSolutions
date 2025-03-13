using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using server;
using server.Properties;
using System.Text.Json;


Database database = new();
var db = database.Connection();
Queries queries = new(db);
Mail newmail = new Mail();
CaseTypeUpdate caseTypeUpdate = new();


var builder = WebApplication.CreateBuilder(args);

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
    // Console.WriteLine(company);
    var companyDetails = await queries.GetCompanyName(company);
    
    // Console.WriteLine(companyDetails);
    return companyDetails;
});



app.MapGet("/api/IsUserCsRep", async (HttpContext context) =>
{
    // this shit does what it supposed to, I hate it but I just want to get the chat done
    // 2hrs wasted because I used MapPost instead of MapGet...
    var csRep = context.Session.GetString("CsRep");
    return JsonSerializer.Serialize(csRep, new JsonSerializerOptions { WriteIndented = true });
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
        ChatId = Convert.ToInt32(context.Session.GetInt32("ChatId"))
    };

    Console.WriteLine("chatID: "+ chatId + " user.ChatID: "+ user.ChatId + " user.CsRep: " + user.CsRep);
    if (chatId == user.ChatId && !user.CsRep)
    {
        return await queries.GetChatHistory(user);
    } 
    if (user.CsRep)
    {
        user.ChatId = chatId;
        return await queries.GetChatHistory(user);
    } 
    
    return "no chat found";

});

app.MapPost("/api/assignticket", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var assignChat = JsonSerializer.Deserialize<User>(body);

    // using Users ID here cause why not
    
    assignChat.Id = Convert.ToInt32(context.Session.GetString("UserId"));
    assignChat.CsRep = Convert.ToBoolean(context.Session.GetString("CsRep"));

    if (assignChat.CsRep)
    {
        await queries.assignChatToCsRep(assignChat);
    }
    
});

app.MapGet("/api/assignNextTicket", async (HttpContext context) =>
{
    User assignChat = new User();
    assignChat.Id = Convert.ToInt32(context.Session.GetString("UserId"));
    assignChat.CsRep = Convert.ToBoolean(context.Session.GetString("CsRep"));
    assignChat.CompanyName = context.Session.GetString("CompanyName");
    
    if (assignChat.CsRep)
    {
        var chatId = await queries.GetChatsForCsRep(assignChat.CompanyName, false, true);
        if(chatId != "")
        {
            assignChat.ChatId = Convert.ToInt32(chatId);
            await queries.assignChatToCsRep(assignChat);
            return JsonSerializer.Serialize(chatId, new JsonSerializerOptions { WriteIndented = true });
        }
        return JsonSerializer.Serialize(chatId, new JsonSerializerOptions { WriteIndented = true });
    }
    return "";
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
        var chats = await queries.GetChatsForCsRep(company, sortingObject.getAllChats, false);
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
        if (loginData == null || string.IsNullOrEmpty(loginData.Email) || loginData.ChatId is null)
        {
            return Results.BadRequest(new { message = "Email and chat are required" });
        }

        string decodedEmail = Uri.UnescapeDataString(loginData.Email);
        Console.WriteLine("email: " + decodedEmail + " chatid: " + loginData.ChatId);
        var user = await queries.ValidateTempUser(decodedEmail, Convert.ToInt32(loginData.ChatId));
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
                        new Claim(ClaimTypes.Email, user.Email), // sparar användarens e-post 
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



// hämtar epost mallen från databasen med hjälp av queries 
app.MapGet("/api/get-email-template", async (HttpContext context) =>
{
    int companyid = Convert.ToInt32(context.Session.GetInt32("company"));
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));
    
    if(isAdmin){
        var template = await queries.GetEmailTemplate(companyid);
        return JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
    }
    return "no access";
});


// tar emot post-förfrågan med en JSON-body som innehåller den nya mallen 
app.MapPost("/api/post-email-template", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var templateData = JsonSerializer.Deserialize<EmailTemplate>(body);
    Console.WriteLine("title: " + templateData.templateTitle + "greting: " + templateData.templateGreeting + "content: " + templateData.templateContent + "singature: " + templateData.templateSignature);
    int companyid = Convert.ToInt32(context.Session.GetInt32("company"));
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));

    if(isAdmin){
        if (templateData == null)
        {
            return Results.BadRequest(new { message = "Template is a must" });
        }

        bool success = await queries.UpdateEmailTemplate(templateData, companyid);
        if (success)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new { message = "Failed to update template" });
    }
    return Results.Unauthorized();
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

            // Fetch the email template
            EmailTemplate template = await queries.GetEmailTemplate(ticketInformation.companyId);

            // Pass the template to the generateNewIssue method
            await newmail.generateNewIssue(ticketInformation, template);

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

app.MapGet("/api/casetypes", async (HttpContext context) =>
{
    var company = context.Session.GetString("CompanyName");
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));
    Console.WriteLine("Company name:" + company + "isAdmin: " + isAdmin);
    
    if (isAdmin)
    {
        var casetypes = await queries.GetCaseTypes(company);
        Console.WriteLine(casetypes);
        return casetypes;
    }
    
    return null;
});

app.MapPost("/api/NewCaseType", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var caseType = JsonSerializer.Deserialize<CaseTypeUpdate>(body);
    
    Console.WriteLine("hit kommer du");
    caseType.Company = context.Session.GetInt32("company");
    
    queries.postNewCasetype(caseType);
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

app.MapDelete("/api/deleteCaseType", async (HttpContext context) =>
{
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var data = JsonSerializer.Deserialize<CaseTypeDelete>(requestBody);
    
    bool isAdmin = Convert.ToBoolean(context.Session.GetString("IsAdmin"));
    if (isAdmin)
    {
        await queries.removeCasetype(data.caseId);
        Console.WriteLine("Case type deleted successfully.");
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

public class CaseTypeUpdate
{
    public string caseType { get; set; }
    public int? Company { get; set; }  // 🆕 Se till att Company är med!
}


public class ChatSortingObject
{
    public bool getAllChats { get; set; }
}

public class CaseTypeDelete
{
    public int caseId { get; set; }
}

public class EmailTemplate
{
    public string templateTitle { get; set; }
    public string templateGreeting { get; set; }
    public string templateContent { get; set; }
    public string templateSignature { get; set; }

}