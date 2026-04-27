using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractCardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalIdCode",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractCardId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractCards_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractCards_Users_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractBuyerSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonalIdCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractBuyerSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractBuyerSubmissions_ContractCards_ContractCardId",
                        column: x => x.ContractCardId,
                        principalTable: "ContractCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractSellerSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    SdkCode = table.Column<string>(type: "text", nullable: true),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CommercialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mileage = table.Column<int>(type: "integer", nullable: false),
                    Vin = table.Column<string>(type: "text", nullable: true),
                    RegistrationCertificate = table.Column<string>(type: "text", nullable: true),
                    TechnicalInspectionValid = table.Column<bool>(type: "boolean", nullable: false),
                    WasDamaged = table.Column<bool>(type: "boolean", nullable: false),
                    DamageKnown = table.Column<bool>(type: "boolean", nullable: true),
                    DefectBrakes = table.Column<bool>(type: "boolean", nullable: false),
                    DefectSafety = table.Column<bool>(type: "boolean", nullable: false),
                    DefectSteering = table.Column<bool>(type: "boolean", nullable: false),
                    DefectExhaust = table.Column<bool>(type: "boolean", nullable: false),
                    DefectLighting = table.Column<bool>(type: "boolean", nullable: false),
                    DefectDetails = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PersonalIdCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSellerSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractSellerSubmissions_ContractCards_ContractCardId",
                        column: x => x.ContractCardId,
                        principalTable: "ContractCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ContractCardId",
                table: "Messages",
                column: "ContractCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractBuyerSubmissions_ContractCardId",
                table: "ContractBuyerSubmissions",
                column: "ContractCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractCards_ConversationId",
                table: "ContractCards",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractCards_InitiatorId",
                table: "ContractCards",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSellerSubmissions_ContractCardId",
                table: "ContractSellerSubmissions",
                column: "ContractCardId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ContractCards_ContractCardId",
                table: "Messages",
                column: "ContractCardId",
                principalTable: "ContractCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ContractCards_ContractCardId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "ContractBuyerSubmissions");

            migrationBuilder.DropTable(
                name: "ContractSellerSubmissions");

            migrationBuilder.DropTable(
                name: "ContractCards");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ContractCardId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonalIdCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContractCardId",
                table: "Messages");
        }
    }
}
