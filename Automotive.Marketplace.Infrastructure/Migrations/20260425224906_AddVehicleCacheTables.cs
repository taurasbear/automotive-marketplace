using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleCacheTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleEfficiencyCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LitersPer100Km = table.Column<double>(type: "double precision", nullable: true),
                    KWhPer100Km = table.Column<double>(type: "double precision", nullable: true),
                    FetchedTrimName = table.Column<string>(type: "text", nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleEfficiencyCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleMarketCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    MedianPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalListings = table.Column<int>(type: "integer", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleMarketCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleReliabilityCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    RecallCount = table.Column<int>(type: "integer", nullable: false),
                    ComplaintCrashes = table.Column<int>(type: "integer", nullable: false),
                    ComplaintInjuries = table.Column<int>(type: "integer", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleReliabilityCaches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleEfficiencyCaches_Make_Model_Year",
                table: "VehicleEfficiencyCaches",
                columns: new[] { "Make", "Model", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMarketCaches_Make_Model_Year",
                table: "VehicleMarketCaches",
                columns: new[] { "Make", "Model", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleReliabilityCaches_Make_Model_Year",
                table: "VehicleReliabilityCaches",
                columns: new[] { "Make", "Model", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleEfficiencyCaches");

            migrationBuilder.DropTable(
                name: "VehicleMarketCaches");

            migrationBuilder.DropTable(
                name: "VehicleReliabilityCaches");
        }
    }
}
