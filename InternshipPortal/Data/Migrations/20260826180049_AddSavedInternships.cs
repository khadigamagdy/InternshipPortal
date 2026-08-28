using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedInternships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipApplications_Internships_InternshipId",
                table: "InternshipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Internships_Companies_CompanyId",
                table: "Internships");

            migrationBuilder.CreateTable(
                name: "SavedInternships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    InternshipId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedInternships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedInternships_Internships_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedInternships_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedInternships_InternshipId",
                table: "SavedInternships",
                column: "InternshipId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedInternships_StudentId_InternshipId",
                table: "SavedInternships",
                columns: new[] { "StudentId", "InternshipId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipApplications_Internships_InternshipId",
                table: "InternshipApplications",
                column: "InternshipId",
                principalTable: "Internships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Internships_Companies_CompanyId",
                table: "Internships",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipApplications_Internships_InternshipId",
                table: "InternshipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Internships_Companies_CompanyId",
                table: "Internships");

            migrationBuilder.DropTable(
                name: "SavedInternships");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipApplications_Internships_InternshipId",
                table: "InternshipApplications",
                column: "InternshipId",
                principalTable: "Internships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Internships_Companies_CompanyId",
                table: "Internships",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
