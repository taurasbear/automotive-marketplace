using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefectCategoryAndListingDefect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ListingDefectId",
                table: "Images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DefectCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefectCategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefectCategoryTranslations_DefectCategories_DefectCategoryId",
                        column: x => x.DefectCategoryId,
                        principalTable: "DefectCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListingDefects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingDefects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingDefects_DefectCategories_DefectCategoryId",
                        column: x => x.DefectCategoryId,
                        principalTable: "DefectCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ListingDefects_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_ListingDefectId",
                table: "Images",
                column: "ListingDefectId");

            migrationBuilder.CreateIndex(
                name: "IX_DefectCategoryTranslations_DefectCategoryId_LanguageCode",
                table: "DefectCategoryTranslations",
                columns: new[] { "DefectCategoryId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListingDefects_DefectCategoryId",
                table: "ListingDefects",
                column: "DefectCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDefects_ListingId",
                table: "ListingDefects",
                column: "ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ListingDefects_ListingDefectId",
                table: "Images",
                column: "ListingDefectId",
                principalTable: "ListingDefects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_ListingDefects_ListingDefectId",
                table: "Images");

            migrationBuilder.DropTable(
                name: "DefectCategoryTranslations");

            migrationBuilder.DropTable(
                name: "ListingDefects");

            migrationBuilder.DropTable(
                name: "DefectCategories");

            migrationBuilder.DropIndex(
                name: "IX_Images_ListingDefectId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ListingDefectId",
                table: "Images");
        }
    }
}
