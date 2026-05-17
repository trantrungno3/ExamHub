namespace ExamHub.Core.Domain.Enums;

/// <summary>
/// Loại câu hỏi trong hệ thống
/// </summary>
public enum QuestionTypeEnum : byte
{
    /// <summary>Trắc nghiệm 1 đáp án</summary>
    MultipleChoice = 1,

    /// <summary>Trắc nghiệm nhiều đáp án</summary>
    MultipleSelect = 2,

    /// <summary>Đúng/Sai</summary>
    TrueFalse = 3,

    /// <summary>Điền vào chỗ trống</summary>
    FillBlank = 4,

    /// <summary>Tự luận</summary>
    Essay = 5,

    /// <summary>Nối cột</summary>
    Matching = 6
}

