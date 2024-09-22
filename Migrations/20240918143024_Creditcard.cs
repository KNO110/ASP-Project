using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_P15.Migrations
{
    /// <inheritdoc />
    public partial class Creditcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardOwner",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpiryDate",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CardOwner",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Users");
        }
    }
}
