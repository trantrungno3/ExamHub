using System.Linq.Expressions;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.Application.Submissions;

/// <summary>
/// Logic thuần về "lượt làm bài đã dùng" — tách khỏi repository để unit-test được không cần DB.
/// </summary>
public static class SubmissionAttempts
{
    /// <summary>
    /// Điều kiện một bản nộp được tính là ĐÃ DÙNG một lượt trong kỳ thi.
    /// Loại trừ theo <see cref="SubmissionStatusEnum.InProgress"/> (thay vì liệt kê từng trạng
    /// thái "đã xong") để mọi trạng thái kết thúc mới — vd. PendingManualGrade — tự động được
    /// tính, không phải sửa lại truy vấn mỗi lần thêm trạng thái.
    /// </summary>
    public static Expression<Func<ExamSubmission, bool>> UsedAttemptFilter(Guid sessionId, Guid studentId)
        => x => x.SessionId == sessionId
                && x.StudentId == studentId
                && x.Status != SubmissionStatusEnum.InProgress;
}
