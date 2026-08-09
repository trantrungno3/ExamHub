using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho Question</summary>
public interface IQuestionService
{
    /// <summary>Lấy câu hỏi theo ID</summary>
    Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy câu hỏi kèm đáp án</summary>
    Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy câu hỏi theo chủ đề</summary>
    Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId, CancellationToken ct = default);

    /// <summary>Lấy câu hỏi phân trang với bộ lọc</summary>
    Task<(IReadOnlyList<Question> Items, int Total)> GetPagedAsync(int page, int pageSize, int? topicId = null, int? questionTypeId = null, int? difficultyLevelId = null, int? cognitiveLevelId = null, string? keyword = null, string? reviewStatus = null, CancellationToken ct = default);

    /// <summary>Tạo câu hỏi kèm đáp án</summary>
    Task<Question> CreateAsync(Question entity, IEnumerable<QuestionAnswer> answers, CancellationToken ct = default);

    /// <summary>Cập nhật câu hỏi (tuỳ chọn cập nhật đáp án)</summary>
    Task<Question> UpdateAsync(Question entity, IEnumerable<QuestionAnswer>? answers = null, CancellationToken ct = default);

    /// <summary>Xóa câu hỏi</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Kiểm duyệt câu hỏi</summary>
    Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default);

    /// <summary>Bỏ duyệt câu hỏi</summary>
    Task UnverifyAsync(Guid id, CancellationToken ct = default);

    /// <summary>Từ chối câu hỏi kèm lý do</summary>
    Task RejectAsync(Guid id, Guid reviewedBy, string reason, CancellationToken ct = default);

    /// <summary>Thống kê số câu hỏi theo trạng thái</summary>
    Task<QuestionStatsResponse> GetStatsAsync(CancellationToken ct = default);

    /// <summary>Cập nhật URL tệp audio đính kèm</summary>
    Task SetAudioUrlAsync(Guid id, string audioUrl, CancellationToken ct = default);

    /// <summary>Gán URL tệp đính kèm (ảnh/PDF) cho câu hỏi.</summary>
    Task SetImageUrlAsync(Guid id, string imageUrl, CancellationToken ct = default);
}

/// <summary>Service interface cho TeacherSubject</summary>
public interface ITeacherSubjectService
{
    /// <summary>Lấy danh sách môn học của giáo viên</summary>
    Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Kiểm tra giáo viên có phụ trách môn học không</summary>
    Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default);

    /// <summary>Gán môn học cho giáo viên</summary>
    Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default);

    /// <summary>Xóa phụ trách môn học</summary>
    Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default);
}

/// <summary>Service interface cho ExamTemplate</summary>
public interface IExamTemplateService
{
    /// <summary>Lấy template theo ID</summary>
    Task<ExamTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy template kèm phần thi</summary>
    Task<ExamTemplate?> GetWithSectionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy danh sách template theo môn học</summary>
    Task<IReadOnlyList<ExamTemplate>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy danh sách template theo lớp</summary>
    Task<IReadOnlyList<ExamTemplate>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default);

    /// <summary>Tạo template kèm phần thi</summary>
    Task<ExamTemplate> CreateAsync(ExamTemplate entity, IEnumerable<ExamTemplateSection> sections, CancellationToken ct = default);

    /// <summary>Cập nhật template (tuỳ chọn cập nhật phần thi)</summary>
    Task<ExamTemplate> UpdateAsync(ExamTemplate entity, IEnumerable<ExamTemplateSection>? sections = null, CancellationToken ct = default);

    /// <summary>Xóa template</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Thống kê mẫu đề thi (tổng, đang dùng, tổng đề sinh, TB số câu)</summary>
    Task<ExamTemplateStatsResponse> GetStatsAsync(CancellationToken ct = default);
}

/// <summary>Service interface cho Exam</summary>
public interface IExamService
{
    /// <summary>Lấy đề thi theo ID</summary>
    Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy đề thi kèm câu hỏi snapshot</summary>
    Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy đề thi phân trang với bộ lọc</summary>
    Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(int page, int pageSize, int? gradeLevelId = null, int? subjectId = null, ExamStatusEnum? status = null, string? keyword = null, CancellationToken ct = default);

    /// <summary>Lấy danh sách đề thi biến thể (cùng lô)</summary>
    Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default);

    /// <summary>Tạo đề thi kèm câu hỏi snapshot</summary>
    Task<Exam> CreateAsync(Exam entity, IEnumerable<ExamQuestion> questions, CancellationToken ct = default);

    /// <summary>Phát hành đề thi (Draft → Published)</summary>
    Task<bool> PublishAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lưu trữ đề thi (Published → Archived)</summary>
    Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default);

    /// <summary>Xóa đề thi</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Thống kê phân bổ câu hỏi trong đề thi theo Bloom / độ khó / chủ đề.
    /// Trả về null nếu đề thi không tồn tại.
    /// </summary>
    Task<DataTransferObjects.Exam.ExamAnalyticsResponse?> GetAnalyticsAsync(Guid examId, CancellationToken ct = default);
}

/// <summary>Service interface cho ExamSubmission</summary>
public interface IExamSubmissionService
{
    /// <summary>Lấy bài nộp theo ID</summary>
    Task<ExamSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy bài nộp kèm câu trả lời</summary>
    Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy danh sách bài nộp theo đề thi</summary>
    Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default);

    /// <summary>Lấy tất cả bài nộp của một học sinh</summary>
    Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default);

    /// <summary>Lấy bài nộp của học sinh theo đề thi</summary>
    Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default);

    /// <summary>Lấy danh sách bài nộp theo kỳ thi</summary>
    Task<IReadOnlyList<ExamSubmission>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>Lấy các lần nộp của một học sinh trong một kỳ thi</summary>
    Task<IReadOnlyList<ExamSubmission>> GetBySessionAndStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);

    /// <summary>Nộp bài kèm câu trả lời</summary>
    Task<ExamSubmission> SubmitAsync(ExamSubmission submission, IEnumerable<SubmissionAnswer> answers, CancellationToken ct = default);

    /// <summary>Chấm điểm một câu trả lời tự luận</summary>
    Task GradeAnswerAsync(Guid submissionAnswerId, decimal scoreEarned, bool isCorrect, string? feedback, Guid gradedBy, CancellationToken ct = default);

    /// <summary>
    /// Chốt điểm bài nộp: cộng tổng <see cref="SubmissionAnswer.ScoreEarned"/> vào
    /// <see cref="ExamSubmission.TotalScore"/> và chuyển trạng thái sang Graded.
    /// </summary>
    Task<ExamSubmission> FinalizeAsync(Guid submissionId, Guid gradedBy, CancellationToken ct = default);
}
