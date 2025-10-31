using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VidSync.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Rooms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Rooms");
        }
    }
}
