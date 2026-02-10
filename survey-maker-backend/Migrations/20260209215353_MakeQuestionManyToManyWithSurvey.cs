using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace survey_maker_backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeQuestionManyToManyWithSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Surveys_SurveyId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SurveyId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "Questions");

            migrationBuilder.CreateTable(
                name: "SurveyQuestions",
                columns: table => new
                {
                    QuestionsId = table.Column<int>(type: "integer", nullable: false),
                    SurveysId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestions", x => new { x.QuestionsId, x.SurveysId });
                    table.ForeignKey(
                        name: "FK_SurveyQuestions_Questions_QuestionsId",
                        column: x => x.QuestionsId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyQuestions_Surveys_SurveysId",
                        column: x => x.SurveysId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestions_SurveysId",
                table: "SurveyQuestions",
                column: "SurveysId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyQuestions");

            migrationBuilder.AddColumn<int>(
                name: "SurveyId",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SurveyId",
                table: "Questions",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Surveys_SurveyId",
                table: "Questions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
