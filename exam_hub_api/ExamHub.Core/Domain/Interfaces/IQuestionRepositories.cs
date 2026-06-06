using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Câu hỏi đã được pick từ pool — chứa đủ dữ liệu để tạo snapshot.</summary>
public sealed record PickedQuestion(Guid QuestionId, string Content, string? AnswersJson);

/// <summary>Interface repository cho Question</summary>
public interface IQuestionRepository : IBaseRepository<Question, Guid>
{
    /// <summary>Lấy câu hỏi kèm đáp án</summary>
    Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy câu hỏi theo chủ đề</summary>
    Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId, bool activeOnly = true, CancellationToken ct = default);

    /// <summary>Lấy pool câu hỏi để sinh đề (kèm filter)</summary>
    Task<IReadOnlyList<Question>> GetPoolAsync(
        int? topicId,
        int? questionTypeId,
        int? difficultyLevelId,
        IEnumerable<Guid>? excludeIds = null,
        CancellationToken ct = default);

    /// <summary>Lấy câu hỏi phân trang</summary>
    Task<(IReadOnlyList<Question> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        int? topicId = null,
        int? questionTypeId = null,
        int? difficultyLevelId = null,
        int? cognitiveLevelId = null,
        string? keyword = null,
        bool? isVerified = null,
        CancellationToken ct = default);

    /// <summary>
    /// Pick ngẫu nhiên N câu hỏi từ pool bằng ORDER BY RANDOM() — dùng Dapper để tránh load toàn bộ pool.
    /// </summary>
    Task<IReadOnlyList<PickedQuestion>> PickRandomAsync(
        int topicId,
        int? questionTypeId,
        int difficultyId,
        int count,
        IReadOnlySet<Guid> excludeIds,
        int? cognitiveLevelId = null,
        CancellationToken ct = default);

    /// <summary>Tăng số lần sử dụng câu hỏi</summary>
    Task IncrementUsageCountAsync(IEnumerable<Guid> questionIds, CancellationToken ct = default);

    /// <summary>Kiểm duyệt câu hỏi</summary>
    Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default);

    /// <summary>Gán URL tệp đính kèm (ảnh/PDF) cho câu hỏi.</summary>
    Task SetImageUrlAsync(Guid id, string imageUrl, CancellationToken ct = default);
}

/// <summary>Interface repository cho QuestionAnswer</summary>
public interface IQuestionAnswerRepository : IBaseRepository<QuestionAnswer, Guid>
{
    /// <summary>Lấy tất cả đáp án của một câu hỏi</summary>
    Task<IReadOnlyList<QuestionAnswer>> GetByQuestionAsync(Guid questionId, CancellationToken ct = default);

    /// <summary>Xóa tất cả đáp án của câu hỏi</summary>
    Task DeleteByQuestionAsync(Guid questionId, CancellationToken ct = default);
}

/// <summary>Interface repository cho TeacherSubject</summary>
public interface ITeacherSubjectRepository : IBaseRepository<TeacherSubject, int>
{
    /// <summary>Lấy danh sách môn học của giáo viên</summary>
    Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Kiểm tra giáo viên có phụ trách môn học không</summary>
    Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default);

    /// <summary>Gán môn học cho giáo viên (upsert)</summary>
    Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default);

    /// <summary>Xóa phụ trách môn học</summary>
    Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default);
}

