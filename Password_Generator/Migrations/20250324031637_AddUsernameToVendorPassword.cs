using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Password_Generator.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToVendorPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "VendorPasswords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "default_username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "VendorPasswords");
        }
    }
}
