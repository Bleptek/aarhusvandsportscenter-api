using Microsoft.EntityFrameworkCore.Migrations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Migrations
{
    public partial class RentalDealSite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DealSite",
                table: "Rentals",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DealSite",
                table: "Rentals");
        }
    }
}
