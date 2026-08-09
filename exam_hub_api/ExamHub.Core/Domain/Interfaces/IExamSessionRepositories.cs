using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Repository cho kỳ thi (exam sessions) — quản lý pool đề, assignment và luồng học sinh.</summary>
public interface IExamSessionRepository
{
    // ── Quản lý ─────────────────────────────────────────────────────────
    /// <summary>Chi tiết kỳ thi kèm Subject, GradeLevel, Exams(+Exam), Assignments.</summary>
    Task<ExamSession?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>Danh sách kỳ thi phân trang kèm Subject/GradeLevel + counts.</summary>
    Task<(IReadOnlyList<ExamSession> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? subjectId, int? gradeLevelId,
        ExamSessionStatusEnum? status, string? keyword, CancellationToken ct = default);

    Task<ExamSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ExamSession s, CancellationToken ct = default);
    Task UpdateAsync(ExamSession s, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SetStatusAsync(Guid id, ExamSessionStatusEnum status, CancellationToken ct = default);

    // ── Pool đề ─────────────────────────────────────────────────────────
    /// <summary>Thêm đề vào pool (bỏ đề đã có).</summary>
    Task AddExamsAsync(Guid sessionId, IEnumerable<Guid> examIds, CancellationToken ct = default);
    Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default);
    /// <summary>Các Exam trong pool của kỳ thi.</summary>
    Task<IReadOnlyList<Exam>> GetPoolExamsAsync(Guid sessionId, CancellationToken ct = default);
    Task<bool> PoolContainsAsync(Guid sessionId, Guid examId, CancellationToken ct = default);

    // ── Assignment ──────────────────────────────────────────────────────
    Task AddAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default);
    Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default);

    /// <summary>Đếm số HS active thuộc phạm vi 1 assignment (cả khoá hoặc 1 lớp/section).</summary>
    Task<int> CountStudentsForAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default);

    // ── Phía học sinh ───────────────────────────────────────────────────
    /// <summary>Kỳ thi published được giao tới học sinh (qua cohort/cohort_class).</summary>
    Task<IReadOnlyList<ExamSession>> GetAssignedToStudentAsync(Guid studentId, CancellationToken ct = default);
    /// <summary>Học sinh có được giao kỳ thi này không.</summary>
    Task<bool> IsStudentAssignedAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);
    /// <summary>Đếm số lượt đã nộp (submitted/graded) của học sinh trong kỳ thi.</summary>
    Task<int> CountSubmittedAttemptsAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);
    /// <summary>Bài nộp đang làm dở (in_progress) của học sinh trong kỳ thi.</summary>
    Task<ExamSubmission?> GetInProgressAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);
    /// <summary>Toàn bộ bài nộp của học sinh trong kỳ thi (để tính trạng thái pool + used attempts).</summary>
    Task<IReadOnlyList<ExamSubmission>> GetStudentSubmissionsAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);

    /// <summary>Tạo bài nộp (insert đơn).</summary>
    Task CreateSubmissionAsync(ExamSubmission submission, CancellationToken ct = default);
}
