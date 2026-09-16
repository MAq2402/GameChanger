namespace GameChanger.Api.Data;

public static class DatabaseConfiguration
{
    public static string GetRequiredConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GameChanger");

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException(
                "The GameChanger database is not configured. Set " +
                "ConnectionStrings:GameChanger with .NET user secrets for local development " +
                "or the ConnectionStrings__GameChanger environment variable for a hosted environment.")
            : connectionString;
    }
}
