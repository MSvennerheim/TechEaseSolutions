

using server;
using server.Properties;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// for testing w. local db
Database database = new();
var db = database.Connection();

app.MapGet("/", () => "Hello World!");
Mail newmail = new Mail();

//newmail.generateNewIssue();
app.Run();
