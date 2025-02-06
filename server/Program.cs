

using server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
Mail newmail = new Mail();

newmail.generateNewIssue();
app.Run();
