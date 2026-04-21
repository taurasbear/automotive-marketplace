using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveYearFromVariantToListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Variants_ModelId_Year_FuelId_TransmissionId_BodyTypeId",
                table: "Variants");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Listings",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """UPDATE "Listings" l SET "Year" = v."Year" FROM "Variants" v WHERE l."VariantId" = v."Id";""");

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                table: "Listings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Variants");

            migrationBuilder.CreateIndex(
                name: "IX_Variants_ModelId_FuelId_TransmissionId_BodyTypeId",
                table: "Variants",
                columns: new[] { "ModelId", "FuelId", "TransmissionId", "BodyTypeId" },
                unique: true,
                filter: "\"IsCustom\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Variants_ModelId_FuelId_TransmissionId_BodyTypeId",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Listings");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Variants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Variants_ModelId_Year_FuelId_TransmissionId_BodyTypeId",
                table: "Variants",
                columns: new[] { "ModelId", "Year", "FuelId", "TransmissionId", "BodyTypeId" },
                unique: true,
                filter: "\"IsCustom\" = false");
        }
    }
}
