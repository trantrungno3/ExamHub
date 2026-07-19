namespace ExamHub.Core.Domain.Enums;

/// <summary>Cách chọn đề khi học sinh vào thi.</summary>
public enum ExamSessionPickModeEnum
{
    /// <summary>Hệ thống bốc ngẫu nhiên.</summary>
    Random,
    /// <summary>Học sinh tự chọn đề trong pool.</summary>
    StudentChoice
}
