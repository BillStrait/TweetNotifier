using Microsoft.EntityFrameworkCore.Migrations;

namespace Dassanie.Migrations
{
    public partial class MorePerfectTwitterIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TwitterId",
                table: "UserDetails",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TwitterId",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");
        }
    }
}
