using Microsoft.EntityFrameworkCore.Migrations;
using Password_Generator.Models;

#nullable disable

namespace Password_Generator.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordCategoryToVendorPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "VendorPasswords",
                type: "int",
                nullable: false,
                defaultValue: (int)PasswordCategory.Other);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "VendorPasswords");
        }
    }
}
