using Microsoft.EntityFrameworkCore;
using Soc.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddDbContext<SocDbContext>(opt =>
opt.UseSqlite("Data Source=soc.db"));

var app = builder.Build();

// Create DB if missing
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<SocDbContext>();
db.Database.EnsureCreated();
}

var apiKey = "soc-dev-key";
app.Use(async (context, next) =>
{
if (context.Request.Path.StartsWithSegments("/api"))
{
if (!context.Request.Headers.TryGetValue("X-API-Key", out var key) || key != apiKey)
{
context.Response.StatusCode = 401;
await context.Response.WriteAsJsonAsync(new { message = "Unauthorized: invalid API key." });
return;
}
}

await next();
});

app.MapControllers();
app.Run();
