namespace ExamHub.Core.Domain.Enums;

/// <summary>
/// Trạng thái đề thi
/// </summary>
public enum ExamStatusEnum : byte
{
    /// <summary>Bản nháp</summary>
    Draft = 1,

    /// <summary>Đã xuất bản</summary>
    Published = 2,

    /// <summary>Đã lưu trữ</summary>
    Archived = 3
}

