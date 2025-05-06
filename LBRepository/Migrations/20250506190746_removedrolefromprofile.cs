using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBRepository.Migrations
{
    /// <inheritdoc />
    public partial class removedrolefromprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Profile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Profile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
