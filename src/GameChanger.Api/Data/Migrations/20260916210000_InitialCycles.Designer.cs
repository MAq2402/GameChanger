using System;
using GameChanger.Api.Domain.Cycles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameChanger.Api.Data.Migrations;

[DbContext(typeof(GameChangerDbContext))]
[Migration("20260916210000_InitialCycles")]
partial class InitialCycles
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.12")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("GameChanger.Api.Domain.Cycles.Cycle", entity =>
        {
            entity.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            entity.Property<DateTimeOffset>("CreatedAtUtc")
                .HasColumnType("datetimeoffset");

            entity.Property<int>("LengthInWeeks")
                .HasColumnType("int");

            entity.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            entity.Property<string>("OwnerId")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            entity.Property<DateOnly>("StartDate")
                .HasColumnType("date");

            entity.Property<CycleStatus>("Status")
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            entity.Property<string>("TimeZoneId")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            entity.HasKey("Id");

            entity.HasIndex("OwnerId", "Status");

            entity.ToTable("Cycles", table =>
            {
                table.HasCheckConstraint(
                    "CK_Cycles_LengthInWeeks",
                    "[LengthInWeeks] >= 1 AND [LengthInWeeks] <= 52");
            });
        });
#pragma warning restore 612, 618
    }
}
