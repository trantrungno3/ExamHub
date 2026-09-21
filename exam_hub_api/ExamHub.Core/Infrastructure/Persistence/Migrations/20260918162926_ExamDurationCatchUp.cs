using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamHub.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExamDurationCatchUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "duration_minutes",
                table: "exam_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 45);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration_minutes",
                table: "exam_sessions");
        }
    }
}
