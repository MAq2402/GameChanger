using System.Net;
using System.Net.Http.Json;
using GameChanger.Api.Data;
using GameChanger.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameChanger.Api.IntegrationTests;

[Collection(SqlServerTestSuite.Name)]
public sealed class ApiIntegrationTests(SqlServerFixture sqlServer)
{
    [Fact]
    public async Task ApiInformationIsAvailable()
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        using var client = factory.CreateClient();

        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.GetAsync("/api/v1", cancellationToken);
        var content = await response.Content.ReadFromJsonAsync<ApiInformation>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal("GameChanger API", content.Name);
        Assert.Equal("v1", content.Version);
    }

    [Fact]
    public async Task HealthEndpointReportsHealthyWhenSqlServerIsAvailable()
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        using var client = factory.CreateClient();

        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.GetAsync("/health", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task EfCoreCanExecuteQueryAgainstContainerDatabase()
    {
        await using var factory = new GameChangerApiFactory(sqlServer.ConnectionString);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameChangerDbContext>();

        var canConnect = await dbContext.Database.CanConnectAsync(TestContext.Current.CancellationToken);

        Assert.True(canConnect);
    }

    private sealed record ApiInformation(string Name, string Version);
}
