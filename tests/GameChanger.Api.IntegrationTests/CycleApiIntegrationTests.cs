using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GameChanger.Api.Data;
using GameChanger.Api.Domain.Cycles;
using GameChanger.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameChanger.Api.IntegrationTests;

[Collection(SqlServerTestSuite.Name)]
public sealed class CycleApiIntegrationTests(SqlServerFixture sqlServer)
{
    [Fact]
    public async Task CreateCyclePersistsDraftAndReturnsItsLocation()
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        await ResetCyclesAsync(factory);
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            "/api/v1/cycles",
            new
            {
                name = "My 10-week reset",
                startDate = "2026-09-21",
                timeZoneId = "Atlantic/Reykjavik",
                lengthInWeeks = 10
            },
            cancellationToken);
        var created = await response.Content.ReadFromJsonAsync<CycleDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("Draft", created.Status);
        Assert.Equal("My 10-week reset", created.Name);
        Assert.Equal($"/api/v1/cycles/{created.Id}", response.Headers.Location?.OriginalString);

        var getResponse = await client.GetAsync(response.Headers.Location, cancellationToken);
        var fetched = await getResponse.Content.ReadFromJsonAsync<CycleDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(created, fetched);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameChangerDbContext>();
        var storedCycle = await dbContext.Cycles.SingleAsync(cancellationToken);

        Assert.Equal("local-development-owner", storedCycle.OwnerId);
        Assert.Equal(10, storedCycle.LengthInWeeks);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(53)]
    public async Task CreateCycleRejectsLengthOutsideAllowedRange(int lengthInWeeks)
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        await ResetCyclesAsync(factory);
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            "/api/v1/cycles",
            new
            {
                name = "Invalid cycle",
                startDate = "2026-09-21",
                timeZoneId = "Atlantic/Reykjavik",
                lengthInWeeks
            },
            cancellationToken);
        using var problem = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(
            problem.RootElement
                .GetProperty("errors")
                .GetProperty("LengthInWeeks")[0]
                .GetString()
                ?.Contains("between 1 and 52", StringComparison.Ordinal));

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameChangerDbContext>();
        Assert.Equal(0, await dbContext.Cycles.CountAsync(cancellationToken));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(52)]
    public async Task CreateCycleAcceptsBoundaryLengths(int lengthInWeeks)
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        await ResetCyclesAsync(factory);
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            "/api/v1/cycles",
            new
            {
                name = "Boundary cycle",
                startDate = "2026-09-21",
                timeZoneId = "Atlantic/Reykjavik",
                lengthInWeeks
            },
            cancellationToken);
        var created = await response.Content.ReadFromJsonAsync<CycleDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(lengthInWeeks, created?.LengthInWeeks);
    }

    [Fact]
    public async Task GetCycleReturnsNotFoundForAnotherOwner()
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        await ResetCyclesAsync(factory);
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameChangerDbContext>();
        var anotherOwnersCycle = Cycle.Create(
            "another-owner",
            "Private cycle",
            new DateOnly(2026, 9, 21),
            "Atlantic/Reykjavik",
            10,
            DateTimeOffset.UtcNow);
        dbContext.Cycles.Add(anotherOwnersCycle);
        await dbContext.SaveChangesAsync(cancellationToken);

        using var client = factory.CreateClient();
        var response = await client.GetAsync(
            $"/api/v1/cycles/{anotherOwnersCycle.Id}",
            cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task ResetCyclesAsync(GameChangerApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameChangerDbContext>();

        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        await dbContext.Cycles.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    private sealed record CycleDto(
        Guid Id,
        string Name,
        DateOnly StartDate,
        string TimeZoneId,
        int LengthInWeeks,
        string Status,
        DateTimeOffset CreatedAtUtc);
}
