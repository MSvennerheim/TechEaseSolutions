using server;
using server.Properties;
using System.Text.Json;
Database database = new();
var db = database.Connection();
Queries queries = new(db);
Mail newmail = new Mail();


var builder = WebApplication.CreateBuilder(args);

/*
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
*/


var app = builder.Build();
// app.UseCors("AllowReactApp");


app.MapGet("/", () => "Hello World!");

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
    
    string emaiConfirmationAdress = await queries.WriteChatToDB(chatData.Message, chatData.Email,  chatData.ChatId, chatData.CaseType, chatData.Company, chatData.Csrep);
    if (emaiConfirmationAdress != "")
    {
        await newmail.emailConfirmationOnAnswer(emaiConfirmationAdress, chatData.ChatId);
    }
    
});


app.MapGet("/api/arbetarsida/{company}", async (string company) =>
{
    var chats = await queries.GetChatsForCsRep(company);
    return chats;
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
//newmail.generateNewIssue();

app.Run();
Console.ReadLine();

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

