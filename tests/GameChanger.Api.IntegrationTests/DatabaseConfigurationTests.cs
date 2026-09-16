using GameChanger.Api.Data;
using Microsoft.Extensions.Configuration;

namespace GameChanger.Api.IntegrationTests;

public sealed class DatabaseConfigurationTests
{
    [Fact]
    public void MissingConnectionStringFailsWithSetupGuidance()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConfiguration.GetRequiredConnectionString(configuration));

        Assert.Contains("ConnectionStrings:GameChanger", exception.Message, StringComparison.Ordinal);
        Assert.Contains("user secrets", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
