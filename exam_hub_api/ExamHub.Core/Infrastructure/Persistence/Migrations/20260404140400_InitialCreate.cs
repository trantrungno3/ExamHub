using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExamHub.Core.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "difficulty_levels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    score_weight = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 1.0m),
                    sort_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_difficulty_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grade_levels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    grade_number = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "question_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    grade_level_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.id);
                    table.ForeignKey(
                        name: "fk_subjects_grade_levels_grade_level_id",
                        column: x => x.grade_level_id,
                        principalTable: "grade_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    grade_level_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 45),
                    total_questions = table.Column<int>(type: "integer", nullable: true),
                    total_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 10.0m),
                    shuffle_questions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    shuffle_answers = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    prevent_duplicate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_templates_grade_levels_grade_level_id",
                        column: x => x.grade_level_id,
                        principalTable: "grade_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exam_templates_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_subjects", x => x.id);
                    table.ForeignKey(
                        name: "fk_teacher_subjects_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topics", x => x.id);
                    table.ForeignKey(
                        name: "fk_topics_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_topics_topics_parent_id",
                        column: x => x.parent_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    grade_level_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    exam_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 45),
                    total_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 10.0m),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    school_year = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    semester = table.Column<short>(type: "smallint", nullable: true),
                    exam_date = table.Column<DateOnly>(type: "date", nullable: true),
                    class_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    parent_exam_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variant_index = table.Column<short>(type: "smallint", nullable: true),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exams", x => x.id);
                    table.ForeignKey(
                        name: "fk_exams_exam_templates_exam_template_id",
                        column: x => x.exam_template_id,
                        principalTable: "exam_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_exams_exams_parent_exam_id",
                        column: x => x.parent_exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exams_grade_levels_grade_level_id",
                        column: x => x.grade_level_id,
                        principalTable: "grade_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exams_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_template_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: true),
                    question_type_id = table.Column<int>(type: "integer", nullable: true),
                    section_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    question_count = table.Column<int>(type: "integer", nullable: false),
                    score_per_question = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    pct_easy = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    pct_medium = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    pct_hard = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    pct_very_hard = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_template_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_template_sections_exam_templates_exam_template_id",
                        column: x => x.exam_template_id,
                        principalTable: "exam_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exam_template_sections_question_types_question_type_id",
                        column: x => x.question_type_id,
                        principalTable: "question_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exam_template_sections_topics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    topic_id = table.Column<int>(type: "integer", nullable: false),
                    question_type_id = table.Column<int>(type: "integer", nullable: false),
                    difficulty_level_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    content_plain = table.Column<string>(type: "text", nullable: true),
                    explanation = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    audio_url = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_questions_difficulty_levels_difficulty_level_id",
                        column: x => x.difficulty_level_id,
                        principalTable: "difficulty_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_questions_question_types_question_type_id",
                        column: x => x.question_type_id,
                        principalTable: "question_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_questions_topics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    total_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    is_passed = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "InProgress"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_submissions_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    content_snapshot = table.Column<string>(type: "text", nullable: false),
                    answers_snapshot = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_questions_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exam_questions_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "question_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    content_plain = table.Column<string>(type: "text", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_question_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submission_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_answer_ids = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    essay_content = table.Column<string>(type: "text", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    score_earned = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 0m),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    graded_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submission_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_submission_answers_exam_questions_exam_question_id",
                        column: x => x.exam_question_id,
                        principalTable: "exam_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_submission_answers_exam_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "exam_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_difficulty_levels_code",
                table: "difficulty_levels",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_questions_exam_id",
                table: "exam_questions",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_questions_question_id",
                table: "exam_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_submissions_exam_id_student_id",
                table: "exam_submissions",
                columns: new[] { "exam_id", "student_id" });

            migrationBuilder.CreateIndex(
                name: "ix_exam_template_sections_exam_template_id",
                table: "exam_template_sections",
                column: "exam_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_template_sections_question_type_id",
                table: "exam_template_sections",
                column: "question_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_template_sections_topic_id",
                table: "exam_template_sections",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_templates_grade_level_id",
                table: "exam_templates",
                column: "grade_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_templates_subject_id",
                table: "exam_templates",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_exams_exam_template_id",
                table: "exams",
                column: "exam_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_exams_grade_level_id",
                table: "exams",
                column: "grade_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_exams_parent_exam_id",
                table: "exams",
                column: "parent_exam_id");

            migrationBuilder.CreateIndex(
                name: "ix_exams_subject_id",
                table: "exams",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_grade_levels_grade_number",
                table: "grade_levels",
                column: "grade_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_question_answers_question_id",
                table: "question_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_types_code",
                table: "question_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_questions_difficulty_level_id",
                table: "questions",
                column: "difficulty_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_questions_question_type_id",
                table: "questions",
                column: "question_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_questions_topic_id",
                table: "questions",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_grade_level_id_code",
                table: "subjects",
                columns: new[] { "grade_level_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_submission_answers_exam_question_id",
                table: "submission_answers",
                column: "exam_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_answers_submission_id",
                table: "submission_answers",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_subject_id",
                table: "teacher_subjects",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_user_id_subject_id",
                table: "teacher_subjects",
                columns: new[] { "user_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_topics_parent_id",
                table: "topics",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_topics_subject_id",
                table: "topics",
                column: "subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_template_sections");

            migrationBuilder.DropTable(
                name: "question_answers");

            migrationBuilder.DropTable(
                name: "submission_answers");

            migrationBuilder.DropTable(
                name: "teacher_subjects");

            migrationBuilder.DropTable(
                name: "exam_questions");

            migrationBuilder.DropTable(
                name: "exam_submissions");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "difficulty_levels");

            migrationBuilder.DropTable(
                name: "question_types");

            migrationBuilder.DropTable(
                name: "topics");

            migrationBuilder.DropTable(
                name: "exam_templates");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "grade_levels");
        }
    }
}
