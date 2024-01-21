using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kalbe.App.InternshipLogbookLogbook.Api.Migrations
{
    public partial class addYearLogbook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "t_Logbook",
                type: "citext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "t_Logbook");
        }
    }
}
