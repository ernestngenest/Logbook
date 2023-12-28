using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kalbe.App.InternshipLogbookLogbook.Api.Migrations
{
    public partial class changeSchoolCodeTostring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "t_Logbook",
                type: "citext",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "SchoolCode",
                table: "t_Logbook",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);
        }
    }
}
