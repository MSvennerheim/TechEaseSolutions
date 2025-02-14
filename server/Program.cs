using server.Properties;

var builder = WebApplication.CreateBuilder(args);

// Lägg till CORS-policy för att tillåta anrop från frontend
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend",
      policy => policy.WithOrigins("http://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddSingleton<Database>();

var app = builder.Build();

app.UseCors("AllowFrontend"); // Använd CORS-policy
app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();
