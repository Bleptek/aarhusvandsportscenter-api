using Microsoft.EntityFrameworkCore.Migrations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Migrations
{
    public partial class RentalProductNamePlural : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamePlural",
                table: "RentalProducts",
                type: "longtext",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamePlural",
                table: "RentalProducts");
        }
    }
}
