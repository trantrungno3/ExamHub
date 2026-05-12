namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng schools
/// </summary>
public readonly struct SchoolTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.schools";

    /// <summary>Tên trường học</summary>
    public const string Name = "name";

    /// <summary>Mã trường: THPT-NGUYEN-DU, ...</summary>
    public const string Code = "code";

    /// <summary>Địa chỉ</summary>
    public const string Address = "address";

    /// <summary>Số điện thoại</summary>
    public const string Phone = "phone";

    /// <summary>Email liên hệ</summary>
    public const string Email = "email";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";

    /// <summary>Ngày tạo</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Ngày cập nhật</summary>
    public const string UpdatedAt = "updated_at";
}
