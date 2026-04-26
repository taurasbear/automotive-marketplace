using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingAiSummaryCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListingAiSummaryCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SummaryType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComparisonListingId = table.Column<Guid>(type: "uuid", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingAiSummaryCaches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingAiSummaryCaches_ListingId_SummaryType_ComparisonList~",
                table: "ListingAiSummaryCaches",
                columns: new[] { "ListingId", "SummaryType", "ComparisonListingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingAiSummaryCaches");
        }
    }
}
