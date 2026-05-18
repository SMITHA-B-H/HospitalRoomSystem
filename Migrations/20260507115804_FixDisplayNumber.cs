using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixDisplayNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "DisplayNumber",
                table: "QueueEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

          
        }

        /// <inheritdoc />
       
    }
}
