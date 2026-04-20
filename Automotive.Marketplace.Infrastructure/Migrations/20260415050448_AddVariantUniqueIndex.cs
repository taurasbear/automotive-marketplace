using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Variants_ModelId",
                table: "Variants");

            migrationBuilder.CreateIndex(
                name: "IX_Variants_ModelId_Year_FuelId_TransmissionId_BodyTypeId",
                table: "Variants",
                columns: new[] { "ModelId", "Year", "FuelId", "TransmissionId", "BodyTypeId" },
                unique: true,
                filter: "\"IsCustom\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Variants_ModelId_Year_FuelId_TransmissionId_BodyTypeId",
                table: "Variants");

            migrationBuilder.CreateIndex(
                name: "IX_Variants_ModelId",
                table: "Variants",
                column: "ModelId");
        }
    }
}
