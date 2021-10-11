using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Migrations
{
    public partial class RentalsAddEndDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetDate",
                table: "Rentals",
                newName: "StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_TargetDate",
                table: "Rentals",
                newName: "IX_Rentals_StartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Rentals",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_EndDate",
                table: "Rentals",
                column: "EndDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rentals_EndDate",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Rentals",
                newName: "TargetDate");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_StartDate",
                table: "Rentals",
                newName: "IX_Rentals_TargetDate");
        }
    }
}
