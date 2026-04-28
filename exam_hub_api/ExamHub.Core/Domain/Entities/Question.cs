using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Câu hỏi trong ngân hàng câu hỏi
/// </summary>
[Table(QuestionTable.TableName)]
[SqlBuilderProperty(QuestionTable.TableName)]
public class Question : IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại chủ đề</summary>
    [Column(QuestionTable.TopicId)]
    [SqlBuilderProperty(QuestionTable.TopicId, Insert = true, Update = true)]
    public int TopicId { get; set; }

    /// <summary>Khóa ngoại loại câu hỏi</summary>
    [Column(QuestionTable.QuestionTypeId)]
    [SqlBuilderProperty(QuestionTable.QuestionTypeId, Insert = true, Update = true)]
    public int QuestionTypeId { get; set; }

    /// <summary>Khóa ngoại mức độ khó</summary>
    [Column(QuestionTable.DifficultyLevelId)]
    [SqlBuilderProperty(QuestionTable.DifficultyLevelId, Insert = true, Update = true)]
    public int DifficultyLevelId { get; set; }

    /// <summary>ID người tạo câu hỏi</summary>
    [Column(QuestionTable.CreatedBy)]
    [SqlBuilderProperty(QuestionTable.CreatedBy, Insert = true, Update = false)]
    public Guid CreatedBy { get; set; }

    /// <summary>Nội dung câu hỏi (HTML/Markdown)</summary>
    [Column(QuestionTable.Content)]
    [SqlBuilderProperty(QuestionTable.Content, Insert = true, Update = true)]
    public required string Content { get; set; }

    /// <summary>Nội dung thuần text để full-text search</summary>
    [Column(QuestionTable.ContentPlain)]
    [SqlBuilderProperty(QuestionTable.ContentPlain, Insert = true, Update = true)]
    public string? ContentPlain { get; set; }

    /// <summary>Giải thích đáp án đúng</summary>
    [Column(QuestionTable.Explanation)]
    [SqlBuilderProperty(QuestionTable.Explanation, Insert = true, Update = true)]
    public string? Explanation { get; set; }

    /// <summary>URL ảnh đính kèm (MinIO)</summary>
    [Column(QuestionTable.ImageUrl)]
    [SqlBuilderProperty(QuestionTable.ImageUrl, Insert = true, Update = true)]
    public string? ImageUrl { get; set; }

    /// <summary>URL audio đính kèm (MinIO)</summary>
    [Column(QuestionTable.AudioUrl)]
    [SqlBuilderProperty(QuestionTable.AudioUrl, Insert = true, Update = true)]
    public string? AudioUrl { get; set; }

    /// <summary>Nguồn câu hỏi (sách giáo khoa, đề thi năm trước...)</summary>
    [Column(QuestionTable.Source)]
    [SqlBuilderProperty(QuestionTable.Source, Insert = true, Update = true)]
    public string? Source { get; set; }

    /// <summary>Tags để phân loại, tìm kiếm</summary>
    [Column(QuestionTable.Tags)]
    [SqlBuilderProperty(QuestionTable.Tags, Insert = true, Update = true)]
    public string[] Tags { get; set; } = [];

    /// <summary>Số lần câu hỏi được dùng trong đề thi</summary>
    [Column(QuestionTable.UsageCount)]
    [SqlBuilderProperty(QuestionTable.UsageCount, Insert = true, Update = true)]
    public int UsageCount { get; set; } = 0;

    /// <summary>Kích hoạt hay không</summary>
    [Column(QuestionTable.IsActive)]
    [SqlBuilderProperty(QuestionTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    /// <summary>Đã được kiểm duyệt hay chưa</summary>
    [Column(QuestionTable.IsVerified)]
    [SqlBuilderProperty(QuestionTable.IsVerified, Insert = true, Update = true)]
    public bool IsVerified { get; set; } = false;

    /// <summary>ID người kiểm duyệt</summary>
    [Column(QuestionTable.VerifiedBy)]
    [SqlBuilderProperty(QuestionTable.VerifiedBy, Insert = true, Update = true)]
    public Guid? VerifiedBy { get; set; }

    /// <summary>Thời điểm kiểm duyệt</summary>
    [Column(QuestionTable.VerifiedAt)]
    [SqlBuilderProperty(QuestionTable.VerifiedAt, Insert = true, Update = true)]
    public DateTime? VerifiedAt { get; set; }

    /// <summary>Thời điểm tạo</summary>
    [Column(QuestionTable.CreatedAt)]
    [SqlBuilderProperty(QuestionTable.CreatedAt, Insert = true, Update = false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời điểm cập nhật gần nhất</summary>
    [Column(QuestionTable.UpdatedAt)]
    [SqlBuilderProperty(QuestionTable.UpdatedAt, Insert = true, Update = true)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Chủ đề của câu hỏi</summary>
    public Topic? Topic { get; set; }

    /// <summary>Loại câu hỏi</summary>
    public QuestionType? QuestionType { get; set; }

    /// <summary>Mức độ khó</summary>
    public DifficultyLevel? DifficultyLevel { get; set; }

    /// <summary>Danh sách đáp án</summary>
    public List<QuestionAnswer> Answers { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id                   = Id,
        topic_id             = TopicId,
        question_type_id     = QuestionTypeId,
        difficulty_level_id  = DifficultyLevelId,
        created_by           = CreatedBy,
        content              = Content,
        content_plain        = ContentPlain,
        explanation          = Explanation,
        image_url            = ImageUrl,
        audio_url            = AudioUrl,
        source               = Source,
        tags                 = Tags,
        usage_count          = UsageCount,
        is_active            = IsActive,
        is_verified          = IsVerified,
        verified_by          = VerifiedBy,
        verified_at          = VerifiedAt,
        created_at           = CreatedAt,
        updated_at           = UpdatedAt
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id                   = Id,
        topic_id             = TopicId,
        question_type_id     = QuestionTypeId,
        difficulty_level_id  = DifficultyLevelId,
        content              = Content,
        content_plain        = ContentPlain,
        explanation          = Explanation,
        image_url            = ImageUrl,
        audio_url            = AudioUrl,
        source               = Source,
        tags                 = Tags,
        usage_count          = UsageCount,
        is_active            = IsActive,
        is_verified          = IsVerified,
        verified_by          = VerifiedBy,
        verified_at          = VerifiedAt,
        updated_at           = DateTime.UtcNow
    };
}
