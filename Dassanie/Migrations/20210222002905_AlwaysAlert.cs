using Microsoft.EntityFrameworkCore.Migrations;

namespace Dassanie.Migrations
{
    public partial class AlwaysAlert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlwaysAlert",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlwaysAlert",
                table: "Alerts");
        }
    }
}
