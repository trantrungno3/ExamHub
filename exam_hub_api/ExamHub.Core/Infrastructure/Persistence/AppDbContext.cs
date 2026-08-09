using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using TVT.Core.IdentityUser.PostgreSql.FieldTables;

namespace ExamHub.Core.Infrastructure.Persistence;

/// <summary>
/// DbContext chính của ứng dụng ExamHub — ánh xạ tất cả entity sang PostgreSQL
/// </summary>
public class AppDbContext : DbContext
{
    /// <inheritdoc />
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ── DbSets ──────────────────────────────────────────────────────────────
    /// <summary>Lớp học (1–12)</summary>
    public DbSet<GradeLevel> GradeLevels { get; set; }

    /// <summary>Môn học</summary>
    public DbSet<Subject> Subjects { get; set; }

    /// <summary>Chủ đề / Chương</summary>
    public DbSet<Topic> Topics { get; set; }

    /// <summary>Mức độ khó</summary>
    public DbSet<DifficultyLevel> DifficultyLevels { get; set; }

    /// <summary>Loại câu hỏi</summary>
    public DbSet<QuestionType> QuestionTypes { get; set; }

    /// <summary>Cấp độ nhận thức Bloom's Taxonomy</summary>
    public DbSet<CognitiveLevel> CognitiveLevels { get; set; }

    /// <summary>Trường học</summary>
    public DbSet<School> Schools { get; set; }

    /// <summary>Khoá học tuyển sinh</summary>
    public DbSet<Cohort> Cohorts { get; set; }

    /// <summary>Lớp học (sinh tự động từ khoá)</summary>
    public DbSet<CohortClass> CohortClasses { get; set; }

    /// <summary>Học sinh thuộc khoá</summary>
    public DbSet<CohortMember> CohortMembers { get; set; }

    /// <summary>Giáo viên/Admin thuộc trường</summary>
    public DbSet<SchoolMember> SchoolMembers { get; set; }

    /// <summary>Câu hỏi trong ngân hàng</summary>
    public DbSet<Question> Questions { get; set; }

    /// <summary>Đáp án câu hỏi</summary>
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }

    /// <summary>Quan hệ giáo viên – môn học</summary>
    public DbSet<TeacherSubject> TeacherSubjects { get; set; }

    /// <summary>Mẫu đề thi</summary>
    public DbSet<ExamTemplate> ExamTemplates { get; set; }

    /// <summary>Phần trong mẫu đề thi</summary>
    public DbSet<ExamTemplateSection> ExamTemplateSections { get; set; }

    /// <summary>Đề thi cụ thể</summary>
    public DbSet<Exam> Exams { get; set; }

    /// <summary>Câu hỏi snapshot trong đề thi</summary>
    public DbSet<ExamQuestion> ExamQuestions { get; set; }

    /// <summary>Bài nộp của học sinh</summary>
    public DbSet<ExamSubmission> ExamSubmissions { get; set; }

    /// <summary>Kỳ thi</summary>
    public DbSet<ExamSession> ExamSessions { get; set; }

    /// <summary>Đề thi thuộc pool của kỳ thi</summary>
    public DbSet<ExamSessionExam> ExamSessionExams { get; set; }

    /// <summary>Giao kỳ thi cho lớp/khoá</summary>
    public DbSet<ExamSessionAssignment> ExamSessionAssignments { get; set; }

    /// <summary>Câu trả lời trong bài nộp</summary>
    public DbSet<SubmissionAnswer> SubmissionAnswers { get; set; }

    // ── Model Configuration ─────────────────────────────────────────────────
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── GradeLevel ─────────────────────────────────────────────────────
        modelBuilder.Entity<GradeLevel>(e =>
        {
            e.ToTable("grade_levels");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.GradeNumber).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => x.GradeNumber).IsUnique();
        });

        // ── Subject ────────────────────────────────────────────────────────
        modelBuilder.Entity<Subject>(e =>
        {
            e.ToTable("subjects");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => new { x.GradeLevelId, x.Code }).IsUnique();
            e.HasOne(x => x.GradeLevel)
                .WithMany(x => x.Subjects)
                .HasForeignKey(x => x.GradeLevelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Topic ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Topic>(e =>
        {
            e.ToTable("topics");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasOne(x => x.Subject)
                .WithMany(x => x.Topics)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── DifficultyLevel ────────────────────────────────────────────────
        modelBuilder.Entity<DifficultyLevel>(e =>
        {
            e.ToTable("difficulty_levels");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.Property(x => x.ScoreWeight).HasPrecision(4, 2).HasDefaultValue(1.0m);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── QuestionType ───────────────────────────────────────────────────
        modelBuilder.Entity<QuestionType>(e =>
        {
            e.ToTable("question_types");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(300);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── CognitiveLevel ─────────────────────────────────────────────────
        modelBuilder.Entity<CognitiveLevel>(e =>
        {
            e.ToTable("cognitive_levels");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(100).IsRequired();
            e.Property(x => x.ColorCode).HasMaxLength(10);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.LevelOrder).IsUnique();
        });

        // ── Question ───────────────────────────────────────────────────────
        modelBuilder.Entity<Question>(e =>
        {
            e.ToTable("questions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.AudioUrl).HasMaxLength(500);
            e.Property(x => x.Source).HasMaxLength(300);
            e.Property(x => x.Tags).HasColumnType("text[]");
            e.Property(x => x.UsageCount).HasDefaultValue(0);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("pending");
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created).HasDefaultValueSql("now()");
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified).HasDefaultValueSql("now()");
            e.HasOne(x => x.Topic)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.TopicId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.QuestionType)
                .WithMany()
                .HasForeignKey(x => x.QuestionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.DifficultyLevel)
                .WithMany()
                .HasForeignKey(x => x.DifficultyLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CognitiveLevel)
                .WithMany()
                .HasForeignKey(x => x.CognitiveLevelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ── QuestionAnswer ─────────────────────────────────────────────────
        modelBuilder.Entity<QuestionAnswer>(e =>
        {
            e.ToTable("question_answers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.IsCorrect).HasDefaultValue(false);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.HasOne(x => x.Question)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TeacherSubject ─────────────────────────────────────────────────
        modelBuilder.Entity<TeacherSubject>(e =>
        {
            e.ToTable("teacher_subjects");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.HasIndex(x => new { x.UserId, x.SubjectId }).IsUnique();
            e.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ExamTemplate ───────────────────────────────────────────────────
        modelBuilder.Entity<ExamTemplate>(e =>
        {
            e.ToTable("exam_templates");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.DurationMinutes).HasDefaultValue(45);
            e.Property(x => x.TotalScore).HasPrecision(6, 2).HasDefaultValue(10.0m);
            e.Property(x => x.ShuffleQuestions).HasDefaultValue(true);
            e.Property(x => x.ShuffleAnswers).HasDefaultValue(true);
            e.Property(x => x.PreventDuplicate).HasDefaultValue(true);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasOne(x => x.GradeLevel)
                .WithMany()
                .HasForeignKey(x => x.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExamTemplateSection ────────────────────────────────────────────
        modelBuilder.Entity<ExamTemplateSection>(e =>
        {
            e.ToTable("exam_template_sections");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SectionName).HasMaxLength(150);
            e.Property(x => x.ScorePerQuestion).HasPrecision(6, 2);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.Property(x => x.PctEasy).HasDefaultValue(0);
            e.Property(x => x.PctMedium).HasDefaultValue(0);
            e.Property(x => x.PctHard).HasDefaultValue(0);
            e.Property(x => x.PctVeryHard).HasDefaultValue(0);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasOne(x => x.ExamTemplate)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.ExamTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Topic)
                .WithMany()
                .HasForeignKey(x => x.TopicId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.QuestionType)
                .WithMany()
                .HasForeignKey(x => x.QuestionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CognitiveLevel)
                .WithMany()
                .HasForeignKey(x => x.CognitiveLevelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ── School ─────────────────────────────────────────────────────────
        modelBuilder.Entity<School>(e =>
        {
            e.ToTable("schools");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── Cohort ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Cohort>(e =>
        {
            e.ToTable("cohorts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.NumClasses).HasDefaultValue((short)1);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => new { x.SchoolId, x.StartYear, x.GradeStart }).IsUnique();
            e.HasOne(x => x.School)
                .WithMany(x => x.Cohorts)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CohortClass ────────────────────────────────────────────────────
        modelBuilder.Entity<CohortClass>(e =>
        {
            e.ToTable("cohort_classes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.ClassName).HasMaxLength(20).IsRequired();
            e.Property(x => x.Section).HasMaxLength(10).IsRequired();
            e.Property(x => x.SchoolYear).HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => new { x.CohortId, x.YearIndex, x.Section }).IsUnique();
            e.HasOne(x => x.Cohort)
                .WithMany(x => x.Classes)
                .HasForeignKey(x => x.CohortId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.GradeLevel)
                .WithMany()
                .HasForeignKey(x => x.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── CohortMember ───────────────────────────────────────────────────
        modelBuilder.Entity<CohortMember>(e =>
        {
            e.ToTable("cohort_members");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.JoinedAt).HasDefaultValueSql("CURRENT_DATE");
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.Section).HasMaxLength(10);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => new { x.CohortId, x.StudentId }).IsUnique();
            e.HasOne(x => x.Cohort)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.CohortId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── SchoolMember ───────────────────────────────────────────────────
        modelBuilder.Entity<SchoolMember>(e =>
        {
            e.ToTable("school_members");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.JoinedAt).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => new { x.SchoolId, x.UserId }).IsUnique();
            e.HasOne(x => x.School)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Exam ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Exam>(e =>
        {
            e.ToTable("exams");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.ExamCode).HasMaxLength(30);
            e.Property(x => x.DurationMinutes).HasDefaultValue(45);
            e.Property(x => x.TotalScore).HasPrecision(6, 2).HasDefaultValue(10.0m);
            e.Property(x => x.Status)
                .HasConversion(new SnakeCaseEnumConverter<ExamStatusEnum>())
                .HasMaxLength(20)
                .HasDefaultValue(ExamStatusEnum.Draft);
            e.Property(x => x.SchoolYear).HasMaxLength(20);
            e.Property(x => x.ClassName).HasMaxLength(50);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            
            e.HasOne(x => x.ExamTemplate)
                .WithMany()
                .HasForeignKey(x => x.ExamTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.GradeLevel)
                .WithMany()
                .HasForeignKey(x => x.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ParentExam)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ParentExamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExamQuestion ───────────────────────────────────────────────────
        modelBuilder.Entity<ExamQuestion>(e =>
        {
            e.ToTable("exam_questions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SectionName).HasMaxLength(150);
            e.Property(x => x.Score).HasPrecision(6, 2);
            e.Property(x => x.ContentSnapshot).IsRequired();
            e.Property(x => x.AnswersSnapshot).HasColumnType("jsonb");
            e.HasOne(x => x.Exam)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExamSubmission ─────────────────────────────────────────────────
        modelBuilder.Entity<ExamSubmission>(e =>
        {
            e.ToTable("exam_submissions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.TotalScore).HasPrecision(6, 2);
            e.Property(x => x.Status)
                .HasConversion(new SnakeCaseEnumConverter<SubmissionStatusEnum>())
                .HasMaxLength(20)
                .HasDefaultValue(SubmissionStatusEnum.InProgress);
            e.Property(x => x.StartedAt).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.Property(x => x.AttemptNo).HasDefaultValue((short)1);
            e.HasIndex(x => new { x.ExamId, x.StudentId });
            e.HasIndex(x => new { x.SessionId, x.StudentId });
            e.HasOne(x => x.Exam)
                .WithMany()
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ExamSession>()
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SubmissionAnswer ───────────────────────────────────────────────
        modelBuilder.Entity<SubmissionAnswer>(e =>
        {
            e.ToTable("submission_answers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SelectedAnswerIds).HasColumnType("uuid[]");
            e.Property(x => x.ScoreEarned).HasPrecision(6, 2).HasDefaultValue(0m);
            e.HasOne(x => x.Submission)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ExamQuestion)
                .WithMany()
                .HasForeignKey(x => x.ExamQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExamSession ────────────────────────────────────────────────────
        modelBuilder.Entity<ExamSession>(e =>
        {
            e.ToTable("exam_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Status)
                .HasConversion(new SnakeCaseEnumConverter<ExamSessionStatusEnum>())
                .HasMaxLength(20)
                .HasDefaultValue(ExamSessionStatusEnum.Draft);
            e.Property(x => x.PickMode)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ExamSessionPickModeEnum.Random);
            e.Property(x => x.MaxAttempts).HasDefaultValue((short)1);
            e.Property(x => x.CreatedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.CreatedBy);
            e.Property(x => x.Created).HasColumnName(ModifyFieldsTable.Created);
            e.Property(x => x.ModifiedBy).HasMaxLength(150).HasColumnName(ModifyFieldsTable.ModifiedBy);
            e.Property(x => x.Modified).HasColumnName(ModifyFieldsTable.Modified);
            e.HasIndex(x => new { x.SubjectId, x.GradeLevelId, x.Status });
            e.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.GradeLevel)
                .WithMany()
                .HasForeignKey(x => x.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExamSessionExam ────────────────────────────────────────────────
        modelBuilder.Entity<ExamSessionExam>(e =>
        {
            e.ToTable("exam_session_exams");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(x => new { x.SessionId, x.ExamId }).IsUnique();
            e.HasOne<ExamSession>()
                .WithMany(s => s.Exams)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Exam)
                .WithMany()
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExamSessionAssignment ──────────────────────────────────────────
        modelBuilder.Entity<ExamSessionAssignment>(e =>
        {
            e.ToTable("exam_session_assignments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(x => x.SessionId);
            e.HasIndex(x => x.CohortId);
            e.HasIndex(x => x.CohortClassId);
            e.HasOne<ExamSession>()
                .WithMany(s => s.Assignments)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Cohort)
                .WithMany()
                .HasForeignKey(x => x.CohortId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.CohortClass)
                .WithMany()
                .HasForeignKey(x => x.CohortClassId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}