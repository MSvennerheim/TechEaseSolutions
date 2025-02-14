var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Add this to support controllers
builder.Services.AddSingleton<Database>(); // Register Database as a singleton

var app = builder.Build();

// Map controllers
app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();
