using GameChanger.Api.Domain.Cycles;
using Microsoft.EntityFrameworkCore;

namespace GameChanger.Api.Data;

public sealed class GameChangerDbContext(DbContextOptions<GameChangerDbContext> options)
    : DbContext(options)
{
    public DbSet<Cycle> Cycles => Set<Cycle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var cycle = modelBuilder.Entity<Cycle>();

        cycle.ToTable("Cycles", table =>
            table.HasCheckConstraint(
                "CK_Cycles_LengthInWeeks",
                "[LengthInWeeks] >= 1 AND [LengthInWeeks] <= 52"));
        cycle.HasKey(entity => entity.Id);
        cycle.Property(entity => entity.OwnerId).HasMaxLength(200).IsRequired();
        cycle.Property(entity => entity.Name)
            .HasMaxLength(Features.Cycles.CreateCycleRequestValidator.MaximumNameLength)
            .IsRequired();
        cycle.Property(entity => entity.StartDate).HasColumnType("date");
        cycle.Property(entity => entity.TimeZoneId)
            .HasMaxLength(Features.Cycles.CreateCycleRequestValidator.MaximumTimeZoneIdLength)
            .IsRequired();
        cycle.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(20);
        cycle.Property(entity => entity.CreatedAtUtc).HasColumnType("datetimeoffset");
        cycle.HasIndex(entity => new { entity.OwnerId, entity.Status });
    }
}
