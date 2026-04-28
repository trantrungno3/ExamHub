using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho ExamTemplate</summary>
public interface IExamTemplateRepository : IBaseRepository<ExamTemplate, Guid>
{
    /// <summary>Lấy template kèm phần thi</summary>
    Task<ExamTemplate?> GetWithSectionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy danh sách template theo môn học</summary>
    Task<IReadOnlyList<ExamTemplate>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy danh sách template theo lớp</summary>
    Task<IReadOnlyList<ExamTemplate>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default);
}

/// <summary>Interface repository cho ExamTemplateSection</summary>
public interface IExamTemplateSectionRepository : IBaseRepository<ExamTemplateSection, Guid>
{
    /// <summary>Lấy danh sách phần thi theo template</summary>
    Task<IReadOnlyList<ExamTemplateSection>> GetByTemplateAsync(Guid templateId, CancellationToken ct = default);

    /// <summary>Xóa tất cả phần thi của template</summary>
    Task DeleteByTemplateAsync(Guid templateId, CancellationToken ct = default);
}

/// <summary>Interface repository cho Exam</summary>
public interface IExamRepository : IBaseRepository<Exam, Guid>
{
    /// <summary>Lấy đề thi kèm câu hỏi snapshot</summary>
    Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy danh sách đề thi theo môn học</summary>
    Task<IReadOnlyList<Exam>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy danh sách đề thi theo trạng thái</summary>
    Task<IReadOnlyList<Exam>> GetByStatusAsync(ExamStatusEnum status, CancellationToken ct = default);

    /// <summary>Lấy danh sách đề thi biến thể (cùng lô)</summary>
    Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default);

    /// <summary>Lấy đề thi phân trang</summary>
    Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        int? gradeLevelId = null,
        int? subjectId = null,
        ExamStatusEnum? status = null,
        string? keyword = null,
        CancellationToken ct = default);

    /// <summary>Cập nhật trạng thái đề thi</summary>
    Task<bool> UpdateStatusAsync(Guid id, ExamStatusEnum status, CancellationToken ct = default);
}

/// <summary>Interface repository cho ExamQuestion</summary>
public interface IExamQuestionRepository : IBaseRepository<ExamQuestion, Guid>
{
    /// <summary>Lấy danh sách câu hỏi snapshot theo đề thi</summary>
    Task<IReadOnlyList<ExamQuestion>> GetByExamAsync(Guid examId, CancellationToken ct = default);

    /// <summary>Xóa tất cả câu hỏi của đề thi</summary>
    Task DeleteByExamAsync(Guid examId, CancellationToken ct = default);
}

/// <summary>Interface repository cho ExamSubmission</summary>
public interface IExamSubmissionRepository : IBaseRepository<ExamSubmission, Guid>
{
    /// <summary>Lấy bài nộp kèm câu trả lời</summary>
    Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lấy danh sách bài nộp theo đề thi</summary>
    Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default);

    /// <summary>Lấy bài nộp của một học sinh theo đề thi</summary>
    Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default);

    /// <summary>Lấy tất cả bài nộp của một học sinh</summary>
    Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default);
}

/// <summary>Interface repository cho SubmissionAnswer</summary>
public interface ISubmissionAnswerRepository : IBaseRepository<SubmissionAnswer, Guid>
{
    /// <summary>Lấy câu trả lời theo bài nộp</summary>
    Task<IReadOnlyList<SubmissionAnswer>> GetBySubmissionAsync(Guid submissionId, CancellationToken ct = default);

    /// <summary>Xóa tất cả câu trả lời của bài nộp</summary>
    Task DeleteBySubmissionAsync(Guid submissionId, CancellationToken ct = default);
}

