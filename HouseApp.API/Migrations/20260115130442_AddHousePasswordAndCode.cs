using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHousePasswordAndCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HouseCode",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Houses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseCode",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Houses");
        }
    }
}
