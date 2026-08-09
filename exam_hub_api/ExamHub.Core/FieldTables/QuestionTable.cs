namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng questions
/// </summary>
public readonly struct QuestionTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.questions";

    /// <summary>Khóa ngoại chủ đề</summary>
    public const string TopicId = "topic_id";

    /// <summary>Khóa ngoại loại câu hỏi</summary>
    public const string QuestionTypeId = "question_type_id";

    /// <summary>Khóa ngoại mức độ khó</summary>
    public const string DifficultyLevelId = "difficulty_level_id";

    /// <summary>Khóa ngoại cấp độ nhận thức Bloom (nullable)</summary>
    public const string CognitiveLevelId = "cognitive_level_id";

    /// <summary>Nội dung câu hỏi (HTML/Markdown)</summary>
    public const string Content = "content";

    /// <summary>Nội dung thuần text để tìm kiếm full-text</summary>
    public const string ContentPlain = "content_plain";

    /// <summary>Giải thích đáp án</summary>
    public const string Explanation = "explanation";

    /// <summary>URL ảnh đính kèm</summary>
    public const string ImageUrl = "image_url";

    /// <summary>URL audio đính kèm</summary>
    public const string AudioUrl = "audio_url";

    /// <summary>Nguồn câu hỏi</summary>
    public const string Source = "source";

    /// <summary>Tags (mảng text)</summary>
    public const string Tags = "tags";

    /// <summary>Số lần đã dùng</summary>
    public const string UsageCount = "usage_count";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";

    /// <summary>Trạng thái duyệt: pending | approved | rejected</summary>
    public const string Status = "status";

    /// <summary>Người kiểm duyệt</summary>
    public const string VerifiedBy = "verified_by";

    /// <summary>Thời điểm kiểm duyệt</summary>
    public const string VerifiedAt = "verified_at";

    /// <summary>Lý do từ chối (≠ null ⇒ câu hỏi bị từ chối)</summary>
    public const string RejectionReason = "rejection_reason";
}

