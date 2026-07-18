using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Chủ đề / Chương học (hỗ trợ phân cấp cha–con)
/// </summary>
[Table(TopicTable.TableName)]
[SqlBuilderProperty(TopicTable.TableName)]
public class Topic : ModifyModelBase, IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Khóa ngoại môn học</summary>
    [Column(TopicTable.SubjectId)]
    [SqlBuilderProperty(TopicTable.SubjectId, Insert = true, Update = true)]
    public int SubjectId { get; set; }

    /// <summary>ID chủ đề cha (null = chủ đề gốc)</summary>
    [Column(TopicTable.ParentId)]
    [SqlBuilderProperty(TopicTable.ParentId, Insert = true, Update = true)]
    public int? ParentId { get; set; }

    /// <summary>Tên chủ đề, VD: "Chương 1: Nguyên tử"</summary>
    [Column(TopicTable.Name)]
    [SqlBuilderProperty(TopicTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Mã chủ đề</summary>
    [Column(TopicTable.Code)]
    [SqlBuilderProperty(TopicTable.Code, Insert = true, Update = true)]
    public string? Code { get; set; }

    /// <summary>Thứ tự hiển thị</summary>
    [Column(TopicTable.SortOrder)]
    [SqlBuilderProperty(TopicTable.SortOrder, Insert = true, Update = true)]
    public int SortOrder { get; set; } = 0;

    /// <summary>Mô tả</summary>
    [Column(TopicTable.Description)]
    [SqlBuilderProperty(TopicTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(TopicTable.IsActive)]
    [SqlBuilderProperty(TopicTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Môn học của chủ đề này</summary>
    public Subject? Subject { get; set; }

    /// <summary>Chủ đề cha</summary>
    public Topic? Parent { get; set; }

    /// <summary>Danh sách chủ đề con</summary>
    public List<Topic> Children { get; set; } = [];

    /// <summary>Danh sách câu hỏi thuộc chủ đề này</summary>
    public List<Question> Questions { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        subject_id  = SubjectId,
        parent_id   = ParentId,
        name        = Name,
        code        = Code,
        sort_order  = SortOrder,
        description = Description,
        is_active   = IsActive
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id          = Id,
        subject_id  = SubjectId,
        parent_id   = ParentId,
        name        = Name,
        code        = Code,
        sort_order  = SortOrder,
        description = Description,
        is_active   = IsActive
    };
}
