using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVpicFieldsAndMakeExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAt",
                table: "Models",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VpicId",
                table: "Models",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VpicName",
                table: "Models",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAt",
                table: "Makes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VpicId",
                table: "Makes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VpicName",
                table: "Makes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MakeExclusions",
                columns: table => new
                {
                    VpicId = table.Column<int>(type: "integer", nullable: false),
                    VpicName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeExclusions", x => x.VpicId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Models_VpicId",
                table: "Models",
                column: "VpicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Makes_VpicId",
                table: "Makes",
                column: "VpicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MakeExclusions");

            migrationBuilder.DropIndex(
                name: "IX_Models_VpicId",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Makes_VpicId",
                table: "Makes");

            migrationBuilder.DropColumn(
                name: "SyncedAt",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "VpicId",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "VpicName",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "SyncedAt",
                table: "Makes");

            migrationBuilder.DropColumn(
                name: "VpicId",
                table: "Makes");

            migrationBuilder.DropColumn(
                name: "VpicName",
                table: "Makes");
        }
    }
}
