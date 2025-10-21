using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VidSync.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomSessionAndActivityTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomSessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    JoinTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LeaveTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantActivities_RoomSessions_RoomSessionId",
                        column: x => x.RoomSessionId,
                        principalTable: "RoomSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantActivities_JoinTime",
                table: "ParticipantActivities",
                column: "JoinTime");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantActivities_RoomSessionId",
                table: "ParticipantActivities",
                column: "RoomSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSessions_RoomId",
                table: "RoomSessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSessions_StartTime",
                table: "RoomSessions",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantActivities");

            migrationBuilder.DropTable(
                name: "RoomSessions");
        }
    }
}
