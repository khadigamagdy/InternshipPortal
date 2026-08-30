using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingLogbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_InternshipApplicationId_ScheduledAt",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStatusHistories_InternshipApplicationId_ChangedAt",
                table: "ApplicationStatusHistories");

            migrationBuilder.CreateTable(
                name: "TrainingEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredHours = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UniversityApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyMentorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyMentorEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    InternshipApplicationId = table.Column<int>(type: "int", nullable: false),
                    UniversitySupervisorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingEnrollments_AspNetUsers_UniversitySupervisorUserId",
                        column: x => x.UniversitySupervisorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingEnrollments_InternshipApplications_InternshipApplicationId",
                        column: x => x.InternshipApplicationId,
                        principalTable: "InternshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingHourEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    TaskTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TaskDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LearnedSkills = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompanyComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrainingEnrollmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingHourEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingHourEntries_TrainingEnrollments_TrainingEnrollmentId",
                        column: x => x.TrainingEnrollmentId,
                        principalTable: "TrainingEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TasksCompleted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SkillsLearned = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Challenges = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    NextWeekPlan = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompanyFeedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompanyRating = table.Column<int>(type: "int", nullable: true),
                    SupervisorFeedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SupervisorRating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupervisorReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrainingEnrollmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyReports_TrainingEnrollments_TrainingEnrollmentId",
                        column: x => x.TrainingEnrollmentId,
                        principalTable: "TrainingEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InternshipApplicationId",
                table: "Interviews",
                column: "InternshipApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistories_InternshipApplicationId",
                table: "ApplicationStatusHistories",
                column: "InternshipApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollments_InternshipApplicationId",
                table: "TrainingEnrollments",
                column: "InternshipApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollments_UniversitySupervisorUserId",
                table: "TrainingEnrollments",
                column: "UniversitySupervisorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingHourEntries_TrainingEnrollmentId",
                table: "TrainingHourEntries",
                column: "TrainingEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_TrainingEnrollmentId_WeekNumber",
                table: "WeeklyReports",
                columns: new[] { "TrainingEnrollmentId", "WeekNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories");

            migrationBuilder.DropTable(
                name: "TrainingHourEntries");

            migrationBuilder.DropTable(
                name: "WeeklyReports");

            migrationBuilder.DropTable(
                name: "TrainingEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_InternshipApplicationId",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStatusHistories_InternshipApplicationId",
                table: "ApplicationStatusHistories");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InternshipApplicationId_ScheduledAt",
                table: "Interviews",
                columns: new[] { "InternshipApplicationId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistories_InternshipApplicationId_ChangedAt",
                table: "ApplicationStatusHistories",
                columns: new[] { "InternshipApplicationId", "ChangedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
