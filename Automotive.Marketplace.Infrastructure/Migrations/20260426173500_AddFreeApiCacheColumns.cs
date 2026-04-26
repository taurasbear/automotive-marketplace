using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFreeApiCacheColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplaintCount",
                table: "VehicleReliabilityCaches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OverallSafetyRating",
                table: "VehicleReliabilityCaches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFetchFailed",
                table: "VehicleMarketCaches",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplaintCount",
                table: "VehicleReliabilityCaches");

            migrationBuilder.DropColumn(
                name: "OverallSafetyRating",
                table: "VehicleReliabilityCaches");

            migrationBuilder.DropColumn(
                name: "IsFetchFailed",
                table: "VehicleMarketCaches");
        }
    }
}
