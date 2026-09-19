using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamHub.Core.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Index cho tìm kiếm câu hỏi. Giữ nguyên semantics `ILIKE '%keyword%'` của repository (khớp một
    /// phần từ, không phân biệt hoa thường) và làm nó dùng được index bằng GIN trigram — đổi sang
    /// to_tsvector/plainto_tsquery sẽ nhanh hơn nhưng mất khả năng khớp một phần từ, tức đổi hành vi
    /// tìm kiếm mà người dùng đang dựa vào.
    ///
    /// Viết bằng raw SQL vì EF không mô hình hoá được extension lẫn operator class của index, nên
    /// migration này không làm lệch model snapshot.
    /// </summary>
    public partial class AlignQuestionSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // pg_trgm là contrib module có sẵn trong image postgres chính thức. CREATE EXTENSION cần
            // quyền superuser — trên database có DBA quản lý thì phải cấp trước khi migrate.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_questions_content_plain_trgm
                ON questions USING gin (content_plain gin_trgm_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_questions_content_plain_trgm;");
            // Không DROP EXTENSION: extension có thể đang được index khác dùng.
        }
    }
}
