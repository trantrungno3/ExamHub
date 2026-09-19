namespace ExamHub.API.Authorization;

/// <summary>
/// Ai được đọc một bài nộp. Tách thành hàm thuần để test được mà không phải dựng fake cho cả
/// IExamSubmissionService — quyết định phân quyền là phần cần test, không phải plumbing controller.
/// </summary>
public static class SubmissionAccess
{
    /// <summary>Vai trò được xem bài nộp của người khác (chấm bài, thống kê).</summary>
    private static readonly string[] GraderRoles = ["Admin", "Teacher"];

    /// <summary>
    /// Default deny: chỉ Admin/Teacher xem được bài của người khác; còn lại phải là chủ bài làm.
    /// Không có UserId trong token cũng bị từ chối.
    /// </summary>
    public static bool CanRead(CurrentUserInfo user, Guid ownerStudentId)
    {
        if (user.Roles?.Any(GraderRoles.Contains) == true) return true;
        return user.UserId is { } id && id == ownerStudentId;
    }
}
