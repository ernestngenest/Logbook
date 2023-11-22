using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kalbe.App.InternshipLogbookLogbook.Api.Migrations
{
    public partial class firstCommit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "t_Logbook",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "citext", nullable: false),
                    Upn = table.Column<string>(type: "citext", nullable: true),
                    DocNo = table.Column<string>(type: "citext", nullable: true),
                    DepartmentName = table.Column<string>(type: "citext", nullable: false),
                    SchoolCode = table.Column<long>(type: "bigint", nullable: false),
                    SchoolName = table.Column<string>(type: "citext", nullable: false),
                    FacultyCode = table.Column<string>(type: "citext", nullable: true),
                    FacultyName = table.Column<string>(type: "citext", nullable: false),
                    Month = table.Column<string>(type: "citext", nullable: false),
                    Allowance = table.Column<long>(type: "bigint", nullable: false),
                    WFHCount = table.Column<int>(type: "integer", nullable: false),
                    WFOCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "citext", nullable: true),
                    CreatedBy = table.Column<string>(type: "citext", nullable: true),
                    CreatedByName = table.Column<string>(type: "citext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "citext", nullable: true),
                    UpdatedByName = table.Column<string>(type: "citext", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Logbook", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_Logger",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppCode = table.Column<string>(type: "citext", nullable: true),
                    ModuleCode = table.Column<string>(type: "citext", nullable: true),
                    DocumentNumber = table.Column<string>(type: "citext", nullable: true),
                    Activity = table.Column<string>(type: "citext", nullable: true),
                    CompanyId = table.Column<string>(type: "citext", nullable: true),
                    LogType = table.Column<string>(type: "citext", nullable: true),
                    Message = table.Column<string>(type: "citext", nullable: true),
                    PayLoad = table.Column<string>(type: "citext", nullable: true),
                    PayLoadType = table.Column<string>(type: "citext", nullable: true),
                    ExternalEntity = table.Column<string>(type: "citext", nullable: true),
                    CreatedBy = table.Column<string>(type: "citext", nullable: true),
                    CreatedByName = table.Column<string>(type: "citext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "citext", nullable: true),
                    UpdatedByName = table.Column<string>(type: "citext", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Logger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogbookDays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Activity = table.Column<string>(type: "citext", nullable: false),
                    Status = table.Column<string>(type: "citext", nullable: true),
                    WorkType = table.Column<string>(type: "citext", nullable: false),
                    AllowanceFee = table.Column<long>(type: "bigint", nullable: false),
                    LogbookId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "citext", nullable: true),
                    CreatedByName = table.Column<string>(type: "citext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "citext", nullable: true),
                    UpdatedByName = table.Column<string>(type: "citext", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogbookDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogbookDays_t_Logbook_LogbookId",
                        column: x => x.LogbookId,
                        principalTable: "t_Logbook",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogbookDays_LogbookId",
                table: "LogbookDays",
                column: "LogbookId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogbookDays");

            migrationBuilder.DropTable(
                name: "t_Logger");

            migrationBuilder.DropTable(
                name: "t_Logbook");
        }
    }
}
