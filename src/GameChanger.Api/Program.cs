using GameChanger.Api.Data;
using GameChanger.Api.Domain.Cycles;
using GameChanger.Api.Features.Cycles;
using GameChanger.Api.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentOwner, CurrentOwner>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var healthChecks = builder.Services.AddHealthChecks();
var connectionString = DatabaseConfiguration.GetRequiredConnectionString(builder.Configuration);

builder.Services.AddDbContext<GameChangerDbContext>(options =>
    options.UseSqlServer(connectionString));

healthChecks.AddDbContextCheck<GameChangerDbContext>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GameChangerDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseCors("Frontend");

app.MapGet("/api/v1", () => Results.Ok(new
{
    name = "GameChanger API",
    version = "v1"
}))
    .WithName("GetApiInformation");

var cycles = app.MapGroup("/api/v1/cycles");

cycles.MapPost("/", async (
    CreateCycleRequest request,
    GameChangerDbContext dbContext,
    ICurrentOwner currentOwner,
    TimeProvider timeProvider,
    CancellationToken cancellationToken) =>
{
    var validationErrors = CreateCycleRequestValidator.Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    if (currentOwner.OwnerId is not { } ownerId)
    {
        return Results.Unauthorized();
    }

    var cycle = Cycle.Create(
        ownerId,
        request.Name!,
        request.StartDate!.Value,
        request.TimeZoneId!,
        request.LengthInWeeks!.Value,
        timeProvider.GetUtcNow());

    dbContext.Cycles.Add(cycle);
    await dbContext.SaveChangesAsync(cancellationToken);

    var response = CycleResponse.FromCycle(cycle);
    return Results.Created($"/api/v1/cycles/{cycle.Id}", response);
})
    .WithName("CreateCycle");

cycles.MapGet("/{cycleId:guid}", async (
    Guid cycleId,
    GameChangerDbContext dbContext,
    ICurrentOwner currentOwner,
    CancellationToken cancellationToken) =>
{
    if (currentOwner.OwnerId is not { } ownerId)
    {
        return Results.Unauthorized();
    }

    var cycle = await dbContext.Cycles
        .AsNoTracking()
        .SingleOrDefaultAsync(
            candidate => candidate.Id == cycleId && candidate.OwnerId == ownerId,
            cancellationToken);

    return cycle is null
        ? Results.NotFound()
        : Results.Ok(CycleResponse.FromCycle(cycle));
})
    .WithName("GetCycle");

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
