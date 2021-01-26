using Microsoft.EntityFrameworkCore.Migrations;

namespace Dassanie.Migrations
{
    public partial class SimplifiedAlertWords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TriggerWords",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriggerWords",
                table: "Alerts");
        }
    }
}
