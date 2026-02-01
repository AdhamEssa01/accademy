using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExamAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttemptsAllowed = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAssignments_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamAssignments_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamAssignments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAssignments_AcademyId_ExamId",
                table: "ExamAssignments",
                columns: new[] { "AcademyId", "ExamId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAssignments_ExamId",
                table: "ExamAssignments",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAssignments_GroupId",
                table: "ExamAssignments",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAssignments_StudentId",
                table: "ExamAssignments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamAssignments");
        }
    }
}
