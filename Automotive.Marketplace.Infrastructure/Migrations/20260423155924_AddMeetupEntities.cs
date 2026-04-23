using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automotive.Marketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetupEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AvailabilityCardId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MeetingId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AvailabilityCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilityCards_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailabilityCards_Users_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    LocationText = table.Column<string>(type: "text", nullable: true),
                    LocationLat = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    LocationLng = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ParentMeetingId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_Meetings_ParentMeetingId",
                        column: x => x.ParentMeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Meetings_Users_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilitySlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailabilityCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilitySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilitySlots_AvailabilityCards_AvailabilityCardId",
                        column: x => x.AvailabilityCardId,
                        principalTable: "AvailabilityCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AvailabilityCardId",
                table: "Messages",
                column: "AvailabilityCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MeetingId",
                table: "Messages",
                column: "MeetingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityCards_ConversationId",
                table: "AvailabilityCards",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityCards_InitiatorId",
                table: "AvailabilityCards",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_AvailabilityCardId",
                table: "AvailabilitySlots",
                column: "AvailabilityCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ConversationId",
                table: "Meetings",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_InitiatorId",
                table: "Meetings",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ParentMeetingId",
                table: "Meetings",
                column: "ParentMeetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AvailabilityCards_AvailabilityCardId",
                table: "Messages",
                column: "AvailabilityCardId",
                principalTable: "AvailabilityCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Meetings_MeetingId",
                table: "Messages",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AvailabilityCards_AvailabilityCardId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Meetings_MeetingId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "AvailabilitySlots");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropTable(
                name: "AvailabilityCards");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AvailabilityCardId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MeetingId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AvailabilityCardId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MeetingId",
                table: "Messages");
        }
    }
}
