using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Password_Generator.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityQuestionsToUserPasswordGenerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ResetQuestion2",
                table: "Password_Generator_Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ResetQuestion1",
                table: "Password_Generator_Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ResetAnswer1",
                table: "Password_Generator_Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResetAnswer2",
                table: "Password_Generator_Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetAnswer1",
                table: "Password_Generator_Users");

            migrationBuilder.DropColumn(
                name: "ResetAnswer2",
                table: "Password_Generator_Users");

            migrationBuilder.AlterColumn<string>(
                name: "ResetQuestion2",
                table: "Password_Generator_Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ResetQuestion1",
                table: "Password_Generator_Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
