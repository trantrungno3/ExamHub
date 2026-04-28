using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho GradeLevel</summary>
public interface IGradeLevelService
{
    /// <summary>Lấy tất cả lớp học</summary>
    Task<IReadOnlyList<GradeLevel>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Lấy danh sách đang kích hoạt</summary>
    Task<IReadOnlyList<GradeLevel>> GetActiveAsync(CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<GradeLevel?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>Lấy kèm môn học</summary>
    Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default);
    /// <summary>Tạo mới</summary>
    Task<GradeLevel> CreateAsync(GradeLevel entity, CancellationToken ct = default);
    /// <summary>Cập nhật</summary>
    Task<GradeLevel> UpdateAsync(GradeLevel entity, CancellationToken ct = default);
    /// <summary>Xóa</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}

/// <summary>Service interface cho Subject</summary>
public interface ISubjectService
{
    /// <summary>Lấy tất cả môn học</summary>
    Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Lấy danh sách đang kích hoạt</summary>
    Task<IReadOnlyList<Subject>> GetActiveAsync(CancellationToken ct = default);
    /// <summary>Lấy theo lớp học</summary>
    Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>Lấy kèm chủ đề</summary>
    Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default);
    /// <summary>Tạo mới</summary>
    Task<Subject> CreateAsync(Subject entity, CancellationToken ct = default);
    /// <summary>Cập nhật</summary>
    Task<Subject> UpdateAsync(Subject entity, CancellationToken ct = default);
    /// <summary>Xóa</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}

/// <summary>Service interface cho Topic</summary>
public interface ITopicService
{
    /// <summary>Lấy tất cả chủ đề</summary>
    Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Lấy theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);
    /// <summary>Lấy chủ đề gốc theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default);
    /// <summary>Lấy chủ đề con</summary>
    Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<Topic?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>Tạo mới</summary>
    Task<Topic> CreateAsync(Topic entity, CancellationToken ct = default);
    /// <summary>Cập nhật</summary>
    Task<Topic> UpdateAsync(Topic entity, CancellationToken ct = default);
    /// <summary>Xóa</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}

/// <summary>Service interface cho DifficultyLevel</summary>
public interface IDifficultyLevelService
{
    /// <summary>Lấy tất cả mức độ khó</summary>
    Task<IReadOnlyList<DifficultyLevel>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Lấy danh sách đang kích hoạt</summary>
    Task<IReadOnlyList<DifficultyLevel>> GetActiveAsync(CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<DifficultyLevel?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>Lấy theo mã</summary>
    Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default);
    /// <summary>Tạo mới</summary>
    Task<DifficultyLevel> CreateAsync(DifficultyLevel entity, CancellationToken ct = default);
    /// <summary>Cập nhật</summary>
    Task<DifficultyLevel> UpdateAsync(DifficultyLevel entity, CancellationToken ct = default);
    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}

/// <summary>Service interface cho QuestionType</summary>
public interface IQuestionTypeService
{
    /// <summary>Lấy tất cả loại câu hỏi</summary>
    Task<IReadOnlyList<QuestionType>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Lấy danh sách đang kích hoạt</summary>
    Task<IReadOnlyList<QuestionType>> GetActiveAsync(CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<QuestionType?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>Lấy theo mã</summary>
    Task<QuestionType?> GetByCodeAsync(string code, CancellationToken ct = default);
    /// <summary>Tạo mới</summary>
    Task<QuestionType> CreateAsync(QuestionType entity, CancellationToken ct = default);
    /// <summary>Cập nhật</summary>
    Task<QuestionType> UpdateAsync(QuestionType entity, CancellationToken ct = default);
    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}
