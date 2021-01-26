using Microsoft.EntityFrameworkCore.Migrations;

namespace Dassanie.Migrations
{
    public partial class MorePerfectAlerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwitterFollowName",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwitterFollowName",
                table: "Alerts");
        }
    }
}
