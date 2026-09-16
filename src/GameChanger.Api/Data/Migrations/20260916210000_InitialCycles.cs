using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameChanger.Api.Data.Migrations;

public partial class InitialCycles : Migration
{
    private static readonly string[] OwnerStatusColumns = ["OwnerId", "Status"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Cycles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LengthInWeeks = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cycles", x => x.Id);
                table.CheckConstraint(
                    name: "CK_Cycles_LengthInWeeks",
                    sql: "[LengthInWeeks] >= 1 AND [LengthInWeeks] <= 52");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Cycles_OwnerId_Status",
            table: "Cycles",
            columns: OwnerStatusColumns);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Cycles");
    }
}
