using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameChanger.Api.Data;

public sealed class GameChangerDbContextFactory : IDesignTimeDbContextFactory<GameChangerDbContext>
{
    public GameChangerDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Server=localhost;Database=GameChanger.Design;Integrated Security=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<GameChangerDbContext>()
            .UseSqlServer(designTimeConnectionString)
            .Options;

        return new GameChangerDbContext(options);
    }
}
