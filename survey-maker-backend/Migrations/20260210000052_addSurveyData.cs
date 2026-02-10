using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace survey_maker_backend.Migrations
{
    /// <inheritdoc />
    public partial class addSurveyData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Surveys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Surveys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SurveyAssignedUsers",
                columns: table => new
                {
                    SurveyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyAssignedUsers", x => new { x.SurveyId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SurveyAssignedUsers_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyAssignedUsers");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Surveys");
        }
    }
}
