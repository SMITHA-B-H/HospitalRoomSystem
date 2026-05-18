using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class QueueStageSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "QueueEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stage",
                table: "QueueEntries");
        }
    }
}
