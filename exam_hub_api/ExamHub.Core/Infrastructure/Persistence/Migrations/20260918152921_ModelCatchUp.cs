using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExamHub.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModelCatchUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "questions");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "topics",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "topics",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "subjects",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "subjects",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "questions",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "questions",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "grade_levels",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "grade_levels",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "exams",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "exams",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "exam_template_sections",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "exam_submissions",
                newName: "created");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "topics",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "topics",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "topics",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "topics",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "subjects",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "subjects",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "subjects",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "subjects",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "questions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "audio_url",
                table: "questions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "questions",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "questions",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<int>(
                name: "cognitive_level_id",
                table: "questions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "questions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "question_types",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "question_types",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "question_types",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "question_types",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "grade_levels",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "grade_levels",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "grade_levels",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "grade_levels",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "exams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "draft",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            // The old snapshot included create_by, but InitialCreate never created it.
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "exams",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "exams",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            // InitialCreate omitted the template timestamps recorded in its target model.
            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "exam_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "exam_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "exam_templates",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "exam_templates",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "exam_template_sections",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<int>(
                name: "cognitive_level_id",
                table: "exam_template_sections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "exam_template_sections",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "exam_template_sections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "exam_template_sections",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "exam_submissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "in_progress",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "InProgress");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "exam_submissions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<short>(
                name: "attempt_no",
                table: "exam_submissions",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "exam_submissions",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "exam_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "exam_submissions",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "session_id",
                table: "exam_submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "difficulty_levels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "difficulty_levels",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "difficulty_levels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "difficulty_levels",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cognitive_levels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    level_order = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    color_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cognitive_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cohort_class_teachers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cohort_class_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cohort_class_teachers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    grade_level_id = table.Column<int>(type: "integer", nullable: false),
                    open_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    close_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_attempts = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    pick_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Random"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_sessions_grade_levels_grade_level_id",
                        column: x => x.grade_level_id,
                        principalTable: "grade_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exam_sessions_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schools",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schools", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_session_exams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_session_exams", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_session_exams_exam_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "exam_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exam_session_exams_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cohorts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    school_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_year = table.Column<short>(type: "smallint", nullable: false),
                    end_year = table.Column<short>(type: "smallint", nullable: false),
                    grade_start = table.Column<short>(type: "smallint", nullable: false),
                    num_classes = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cohorts", x => x.id);
                    table.ForeignKey(
                        name: "fk_cohorts_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "school_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    school_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_school_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_school_members_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cohort_classes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cohort_id = table.Column<int>(type: "integer", nullable: false),
                    grade_level_id = table.Column<int>(type: "integer", nullable: false),
                    class_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    school_year = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    year_index = table.Column<short>(type: "smallint", nullable: false),
                    homeroom_teacher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cohort_classes", x => x.id);
                    table.ForeignKey(
                        name: "fk_cohort_classes_cohorts_cohort_id",
                        column: x => x.cohort_id,
                        principalTable: "cohorts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cohort_classes_grade_levels_grade_level_id",
                        column: x => x.grade_level_id,
                        principalTable: "grade_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cohort_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cohort_id = table.Column<int>(type: "integer", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    joined_at = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cohort_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_cohort_members_cohorts_cohort_id",
                        column: x => x.cohort_id,
                        principalTable: "cohorts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_session_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cohort_id = table.Column<int>(type: "integer", nullable: true),
                    cohort_class_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_session_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_exam_session_assignments_cohort_classes_cohort_class_id",
                        column: x => x.cohort_class_id,
                        principalTable: "cohort_classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_exam_session_assignments_cohorts_cohort_id",
                        column: x => x.cohort_id,
                        principalTable: "cohorts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_exam_session_assignments_exam_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "exam_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_questions_cognitive_level_id",
                table: "questions",
                column: "cognitive_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_template_sections_cognitive_level_id",
                table: "exam_template_sections",
                column: "cognitive_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_cognitive_levels_code",
                table: "cognitive_levels",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cognitive_levels_level_order",
                table: "cognitive_levels",
                column: "level_order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cohort_class_teachers_cohort_class_id_subject_id",
                table: "cohort_class_teachers",
                columns: new[] { "cohort_class_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cohort_classes_cohort_id_year_index_section",
                table: "cohort_classes",
                columns: new[] { "cohort_id", "year_index", "section" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cohort_classes_grade_level_id",
                table: "cohort_classes",
                column: "grade_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_cohort_members_cohort_id_student_id",
                table: "cohort_members",
                columns: new[] { "cohort_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cohorts_school_id_start_year_grade_start",
                table: "cohorts",
                columns: new[] { "school_id", "start_year", "grade_start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_session_assignments_cohort_class_id",
                table: "exam_session_assignments",
                column: "cohort_class_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_session_assignments_cohort_id",
                table: "exam_session_assignments",
                column: "cohort_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_session_assignments_session_id",
                table: "exam_session_assignments",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_session_exams_exam_id",
                table: "exam_session_exams",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_session_exams_session_id_exam_id",
                table: "exam_session_exams",
                columns: new[] { "session_id", "exam_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_sessions_grade_level_id",
                table: "exam_sessions",
                column: "grade_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_exam_sessions_subject_id_grade_level_id_status",
                table: "exam_sessions",
                columns: new[] { "subject_id", "grade_level_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_school_members_school_id_user_id",
                table: "school_members",
                columns: new[] { "school_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_schools_code",
                table: "schools",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_exam_submissions_public_exam_sessions_session_id",
                table: "exam_submissions",
                column: "session_id",
                principalTable: "exam_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_exam_template_sections_cognitive_levels_cognitive_level_id",
                table: "exam_template_sections",
                column: "cognitive_level_id",
                principalTable: "cognitive_levels",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_questions_cognitive_levels_cognitive_level_id",
                table: "questions",
                column: "cognitive_level_id",
                principalTable: "cognitive_levels",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_exam_submissions_public_exam_sessions_session_id",
                table: "exam_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_exam_template_sections_cognitive_levels_cognitive_level_id",
                table: "exam_template_sections");

            migrationBuilder.DropForeignKey(
                name: "fk_questions_cognitive_levels_cognitive_level_id",
                table: "questions");

            migrationBuilder.DropTable(
                name: "cognitive_levels");

            migrationBuilder.DropTable(
                name: "cohort_class_teachers");

            migrationBuilder.DropTable(
                name: "cohort_members");

            migrationBuilder.DropTable(
                name: "exam_session_assignments");

            migrationBuilder.DropTable(
                name: "exam_session_exams");

            migrationBuilder.DropTable(
                name: "school_members");

            migrationBuilder.DropTable(
                name: "cohort_classes");

            migrationBuilder.DropTable(
                name: "exam_sessions");

            migrationBuilder.DropTable(
                name: "cohorts");

            migrationBuilder.DropTable(
                name: "schools");

            migrationBuilder.DropIndex(
                name: "ix_questions_cognitive_level_id",
                table: "questions");

            migrationBuilder.DropIndex(
                name: "ix_exam_template_sections_cognitive_level_id",
                table: "exam_template_sections");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "topics");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "topics");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "cognitive_level_id",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "created",
                table: "question_types");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "question_types");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "question_types");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "question_types");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "grade_levels");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "grade_levels");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "exam_templates");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "exam_templates");

            migrationBuilder.DropColumn(
                name: "cognitive_level_id",
                table: "exam_template_sections");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "exam_template_sections");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "exam_template_sections");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "exam_template_sections");

            migrationBuilder.DropColumn(
                name: "attempt_no",
                table: "exam_submissions");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "exam_submissions");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "exam_submissions");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "exam_submissions");

            migrationBuilder.DropColumn(
                name: "session_id",
                table: "exam_submissions");

            migrationBuilder.DropColumn(
                name: "created",
                table: "difficulty_levels");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "difficulty_levels");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "difficulty_levels");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "difficulty_levels");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "topics",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "topics",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "subjects",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "subjects",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "questions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "questions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "grade_levels",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "grade_levels",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "exams",
                newName: "updated_at");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "exams");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "exams",
                newName: "created_at");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "exam_templates");

            migrationBuilder.DropColumn(
                name: "created",
                table: "exam_templates");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "exam_template_sections",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "exam_submissions",
                newName: "created_at");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "topics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "topics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "subjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "subjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "questions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "audio_url",
                table: "questions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "questions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "questions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "grade_levels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "grade_levels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "exams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "draft");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "exams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "exams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "exam_template_sections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "exam_submissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "InProgress",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "in_progress");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "exam_submissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
