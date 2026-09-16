using GameChanger.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var healthChecks = builder.Services.AddHealthChecks();
var connectionString = builder.Configuration.GetConnectionString("GameChanger");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<GameChangerDbContext>(options =>
        options.UseSqlServer(connectionString));

    healthChecks.AddDbContextCheck<GameChangerDbContext>("database");
}

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/api", () => Results.Ok(new
    {
        name = "GameChanger API",
        version = "v1"
    }))
    .WithName("GetApiInformation");

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
