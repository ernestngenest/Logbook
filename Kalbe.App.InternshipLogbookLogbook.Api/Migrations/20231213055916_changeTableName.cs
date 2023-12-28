using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kalbe.App.InternshipLogbookLogbook.Api.Migrations
{
    public partial class changeTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogbookDays_t_Logbook_LogbookId",
                table: "LogbookDays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogbookDays",
                table: "LogbookDays");

            migrationBuilder.RenameTable(
                name: "LogbookDays",
                newName: "d_LogbookDays");

            migrationBuilder.RenameIndex(
                name: "IX_LogbookDays_LogbookId",
                table: "d_LogbookDays",
                newName: "IX_d_LogbookDays_LogbookId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_d_LogbookDays",
                table: "d_LogbookDays",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_d_LogbookDays_t_Logbook_LogbookId",
                table: "d_LogbookDays",
                column: "LogbookId",
                principalTable: "t_Logbook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_d_LogbookDays_t_Logbook_LogbookId",
                table: "d_LogbookDays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_d_LogbookDays",
                table: "d_LogbookDays");

            migrationBuilder.RenameTable(
                name: "d_LogbookDays",
                newName: "LogbookDays");

            migrationBuilder.RenameIndex(
                name: "IX_d_LogbookDays_LogbookId",
                table: "LogbookDays",
                newName: "IX_LogbookDays_LogbookId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogbookDays",
                table: "LogbookDays",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LogbookDays_t_Logbook_LogbookId",
                table: "LogbookDays",
                column: "LogbookId",
                principalTable: "t_Logbook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
