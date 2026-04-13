using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedMakeModelProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstAppearanceDate",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "IsDiscontinued",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "FirstAppearanceDate",
                table: "Makes");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "Makes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FirstAppearanceDate",
                table: "Models",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscontinued",
                table: "Models",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FirstAppearanceDate",
                table: "Makes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                table: "Makes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
