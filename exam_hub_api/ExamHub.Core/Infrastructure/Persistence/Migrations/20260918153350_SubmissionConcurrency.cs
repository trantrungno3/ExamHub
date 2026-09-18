using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamHub.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SubmissionConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_exam_submissions_session_id_student_id",
                table: "exam_submissions",
                columns: new[] { "session_id", "student_id" },
                unique: true,
                filter: "\"session_id\" IS NOT NULL AND \"status\" = 'in_progress'");

            migrationBuilder.CreateIndex(
                name: "ix_exam_submissions_session_id_student_id_attempt_no",
                table: "exam_submissions",
                columns: new[] { "session_id", "student_id", "attempt_no" },
                unique: true,
                filter: "\"session_id\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_exam_submissions_session_id_student_id",
                table: "exam_submissions");

            migrationBuilder.DropIndex(
                name: "ix_exam_submissions_session_id_student_id_attempt_no",
                table: "exam_submissions");
        }
    }
}
