using server;
using server.Properties;


Database database = new();
var db = database.Connection();

Queries queries = new(db);


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.MapGet("/", () => "Hello World!");
Mail newmail = new Mail();

//newmail.generateNewIssue();
app.Run();
Console.ReadLine();
