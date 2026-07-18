using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Loại câu hỏi trong hệ thống
/// </summary>
[Table(QuestionTypeTable.TableName)]
[SqlBuilderProperty(QuestionTypeTable.TableName)]
public class QuestionType : ModifyModelBase, IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Mã loại câu hỏi, VD: "multiple_choice", "essay"</summary>
    [Column(QuestionTypeTable.Code)]
    [SqlBuilderProperty(QuestionTypeTable.Code, Insert = true, Update = false)]
    public required string Code { get; set; }

    /// <summary>Tên loại câu hỏi, VD: "Trắc nghiệm 1 đáp án"</summary>
    [Column(QuestionTypeTable.Name)]
    [SqlBuilderProperty(QuestionTypeTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Mô tả</summary>
    [Column(QuestionTypeTable.Description)]
    [SqlBuilderProperty(QuestionTypeTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(QuestionTypeTable.IsActive)]
    [SqlBuilderProperty(QuestionTypeTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        code        = Code,
        name        = Name,
        description = Description,
        is_active   = IsActive
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id          = Id,
        name        = Name,
        description = Description,
        is_active   = IsActive
    };
}
