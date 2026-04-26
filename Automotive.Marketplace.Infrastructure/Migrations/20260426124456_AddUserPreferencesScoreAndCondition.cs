using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferencesScoreAndCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListingAiSummaryCaches_ListingId_SummaryType_ComparisonList~",
                table: "ListingAiSummaryCaches");

            migrationBuilder.AddColumn<double>(
                name: "ConditionWeight",
                table: "UserPreferences",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableVehicleScoring",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ListingAiSummaryCaches",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "lt");

            migrationBuilder.CreateIndex(
                name: "IX_ListingAiSummaryCaches_ListingId_SummaryType_ComparisonList~",
                table: "ListingAiSummaryCaches",
                columns: new[] { "ListingId", "SummaryType", "ComparisonListingId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListingAiSummaryCaches_ListingId_SummaryType_ComparisonList~",
                table: "ListingAiSummaryCaches");

            migrationBuilder.DropColumn(
                name: "ConditionWeight",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "EnableVehicleScoring",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ListingAiSummaryCaches");

            migrationBuilder.CreateIndex(
                name: "IX_ListingAiSummaryCaches_ListingId_SummaryType_ComparisonList~",
                table: "ListingAiSummaryCaches",
                columns: new[] { "ListingId", "SummaryType", "ComparisonListingId" },
                unique: true);
        }
    }
}
