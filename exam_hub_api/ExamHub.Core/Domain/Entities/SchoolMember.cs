using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Giáo viên / Admin thuộc trường.
/// Role ở đây là vai trò ngữ cảnh trong trường — khác với roles[] JWT toàn hệ thống.
/// </summary>
[Table(SchoolMemberTable.TableName)]
[SqlBuilderProperty(SchoolMemberTable.TableName)]
public class SchoolMember : ModifyModelBase, IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại trường học</summary>
    [Column(SchoolMemberTable.SchoolId)]
    [SqlBuilderProperty(SchoolMemberTable.SchoolId, Insert = true, Update = false)]
    public int SchoolId { get; set; }

    /// <summary>ID người dùng (app_users)</summary>
    [Column(SchoolMemberTable.UserId)]
    [SqlBuilderProperty(SchoolMemberTable.UserId, Insert = true, Update = false)]
    public Guid UserId { get; set; }

    /// <summary>Vai trò trong trường: Admin hoặc Teacher</summary>
    [Column(SchoolMemberTable.Role)]
    [SqlBuilderProperty(SchoolMemberTable.Role, Insert = true, Update = true)]
    public required string Role { get; set; }

    /// <summary>Đang hoạt động</summary>
    [Column(SchoolMemberTable.IsActive)]
    [SqlBuilderProperty(SchoolMemberTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    /// <summary>Thời điểm tham gia trường</summary>
    [Column(SchoolMemberTable.JoinedAt)]
    [SqlBuilderProperty(SchoolMemberTable.JoinedAt, Insert = true, Update = false)]
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Trường học</summary>
    public School? School { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id        = Id,
        school_id = SchoolId,
        user_id   = UserId,
        role      = Role,
        is_active = IsActive,
        joined_at = JoinedAt
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id        = Id,
        role      = Role,
        is_active = IsActive
    };
}
