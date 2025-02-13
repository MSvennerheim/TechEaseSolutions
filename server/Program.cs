using server;
using server.Properties;
using System.Text.Json;
Database database = new();
var db = database.Connection();
Queries queries = new(db);

var builder = WebApplication.CreateBuilder(args);
/*
 
 app.MapGet("/Chat/{chatId:int}", async (int chatId) =>
{
    var chatHistory = await queries.GetChatHistory(chatId);
    return chatHistory;
});

app.MapGet("/arbetarsida/{company}", async (string company) =>
{
    var chats = await queries.GetChatsForCsRep(company);
    return chats;
});

 */

var app = builder.Build();


app.MapGet("/", () => "Hello World!");


app.MapGet("/kontaktaoss/{company}", async (string company) =>
{
    var name = await queries.getCompanyName(company);
    return name;
});


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
            //Populate the Ticket class further and push it to DB
            queries.customerTempUser(ticketInformation);
            await queries.postNewTicket(ticketInformation);
            
            // After successfull Insert, send an email the user wrote in the form
            Mail newmail = new Mail();
            //newmail.generateNewIssue(ticketInformation);
            
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

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
