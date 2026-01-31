using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AcademyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_AcademyId_GroupId",
                table: "Enrollments",
                columns: new[] { "AcademyId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_AcademyId_StudentId",
                table: "Enrollments",
                columns: new[] { "AcademyId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_AcademyId_StudentId_GroupId_StartDate",
                table: "Enrollments",
                columns: new[] { "AcademyId", "StudentId", "GroupId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_GroupId",
                table: "Enrollments",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");
        }
    }
}
