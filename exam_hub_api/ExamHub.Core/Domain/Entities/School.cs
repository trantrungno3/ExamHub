using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Thông tin trường học
/// </summary>
[Table(SchoolTable.TableName)]
[SqlBuilderProperty(SchoolTable.TableName)]
public class School : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty("id", Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Tên trường học</summary>
    [Column(SchoolTable.Name)]
    [SqlBuilderProperty(SchoolTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Mã trường: THPT-NGUYEN-DU, TH-CHU-VAN-AN</summary>
    [Column(SchoolTable.Code)]
    [SqlBuilderProperty(SchoolTable.Code, Insert = true, Update = true)]
    public required string Code { get; set; }

    /// <summary>Địa chỉ</summary>
    [Column(SchoolTable.Address)]
    [SqlBuilderProperty(SchoolTable.Address, Insert = true, Update = true)]
    public string? Address { get; set; }

    /// <summary>Số điện thoại liên hệ</summary>
    [Column(SchoolTable.Phone)]
    [SqlBuilderProperty(SchoolTable.Phone, Insert = true, Update = true)]
    public string? Phone { get; set; }

    /// <summary>Email liên hệ</summary>
    [Column(SchoolTable.Email)]
    [SqlBuilderProperty(SchoolTable.Email, Insert = true, Update = true)]
    public string? Email { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(SchoolTable.IsActive)]
    [SqlBuilderProperty(SchoolTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    /// <summary>Thời điểm tạo</summary>
    [Column(SchoolTable.CreatedAt)]
    [SqlBuilderProperty(SchoolTable.CreatedAt, Insert = true, Update = false)]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Thời điểm cập nhật gần nhất</summary>
    [Column(SchoolTable.UpdatedAt)]
    [SqlBuilderProperty(SchoolTable.UpdatedAt, Insert = true, Update = true)]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Danh sách khoá học</summary>
    public List<Cohort> Cohorts { get; set; } = [];

    /// <summary>Danh sách thành viên (giáo viên/admin)</summary>
    public List<SchoolMember> Members { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        name       = Name,
        code       = Code,
        address    = Address,
        phone      = Phone,
        email      = Email,
        is_active  = IsActive,
        created_at = CreatedAt,
        updated_at = UpdatedAt
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id         = Id,
        name       = Name,
        address    = Address,
        phone      = Phone,
        email      = Email,
        is_active  = IsActive,
        updated_at = DateTimeOffset.UtcNow
    };
}
