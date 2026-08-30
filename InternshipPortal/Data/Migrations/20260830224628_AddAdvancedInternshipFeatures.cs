using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedInternshipFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeeklyReports_TrainingEnrollmentId_WeekNumber",
                table: "WeeklyReports");

            migrationBuilder.CreateTable(
                name: "SkillDevelopmentPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    InternshipId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillDevelopmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillDevelopmentPlans_Internships_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillDevelopmentPlans_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentPortfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Headline = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    SkillsSummary = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    GitHubUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PersonalWebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PortfolioSlug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPortfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPortfolios_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LearningGoal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LearningResourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    SkillDevelopmentPlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillPlanItems_SkillDevelopmentPlans_SkillDevelopmentPlanId",
                        column: x => x.SkillDevelopmentPlanId,
                        principalTable: "SkillDevelopmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    Technologies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProjectUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RepositoryUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentPortfolioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioProjects_StudentPortfolios_StudentPortfolioId",
                        column: x => x.StudentPortfolioId,
                        principalTable: "StudentPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_TrainingEnrollmentId",
                table: "WeeklyReports",
                column: "TrainingEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_StudentPortfolioId_Title",
                table: "PortfolioProjects",
                columns: new[] { "StudentPortfolioId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillDevelopmentPlans_InternshipId",
                table: "SkillDevelopmentPlans",
                column: "InternshipId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillDevelopmentPlans_StudentId_InternshipId",
                table: "SkillDevelopmentPlans",
                columns: new[] { "StudentId", "InternshipId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillPlanItems_SkillDevelopmentPlanId_SkillName",
                table: "SkillPlanItems",
                columns: new[] { "SkillDevelopmentPlanId", "SkillName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPortfolios_PortfolioSlug",
                table: "StudentPortfolios",
                column: "PortfolioSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPortfolios_StudentId",
                table: "StudentPortfolios",
                column: "StudentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioProjects");

            migrationBuilder.DropTable(
                name: "SkillPlanItems");

            migrationBuilder.DropTable(
                name: "StudentPortfolios");

            migrationBuilder.DropTable(
                name: "SkillDevelopmentPlans");

            migrationBuilder.DropIndex(
                name: "IX_WeeklyReports_TrainingEnrollmentId",
                table: "WeeklyReports");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_TrainingEnrollmentId_WeekNumber",
                table: "WeeklyReports",
                columns: new[] { "TrainingEnrollmentId", "WeekNumber" },
                unique: true);
        }
    }
}
