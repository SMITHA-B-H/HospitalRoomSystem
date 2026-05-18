using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNumberToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayNumber",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_DoctorId",
                table: "QueueEntries",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Users_DoctorId",
                table: "QueueEntries",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Users_DoctorId",
                table: "QueueEntries");

            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_DoctorId",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "DisplayNumber",
                table: "Doctors");
        }
    }
}
