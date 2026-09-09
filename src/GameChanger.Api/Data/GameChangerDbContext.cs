using Microsoft.EntityFrameworkCore;

namespace GameChanger.Api.Data;

public sealed class GameChangerDbContext(DbContextOptions<GameChangerDbContext> options)
    : DbContext(options);
