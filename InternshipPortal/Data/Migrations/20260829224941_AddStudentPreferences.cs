using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Students");

            migrationBuilder.CreateTable(
                name: "StudentPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Skills = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CareerInterests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreferredWorkMode = table.Column<int>(type: "int", nullable: true),
                    MinimumSalary = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AcceptUnpaidInternships = table.Column<bool>(type: "bit", nullable: false),
                    AcceptRemoteInternships = table.Column<bool>(type: "bit", nullable: false),
                    MaximumWeeklyHours = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPreferences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentPreferences_StudentId",
                table: "StudentPreferences",
                column: "StudentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories");

            migrationBuilder.DropTable(
                name: "StudentPreferences");

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStatusHistories_AspNetUsers_ChangedByUserId",
                table: "ApplicationStatusHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
