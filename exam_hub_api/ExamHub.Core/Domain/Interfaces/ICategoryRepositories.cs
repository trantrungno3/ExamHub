using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho GradeLevel</summary>
public interface IGradeLevelRepository : ICategoryRepository<GradeLevel, int>
{
    /// <summary>Lấy GradeLevel kèm danh sách Subject</summary>
    Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default);
}

/// <summary>Interface repository cho Subject</summary>
public interface ISubjectRepository : ICategoryRepository<Subject, int>
{
    /// <summary>Lấy danh sách môn học theo lớp</summary>
    Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default);

    /// <summary>Lấy môn học kèm danh sách chủ đề</summary>
    Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default);
}

/// <summary>Interface repository cho Topic</summary>
public interface ITopicRepository : ICategoryRepository<Topic, int>
{
    /// <summary>Lấy danh sách chủ đề theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy chủ đề con theo chủ đề cha</summary>
    Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default);

    /// <summary>Lấy chủ đề gốc (không có cha) theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default);
}

/// <summary>Interface repository cho DifficultyLevel</summary>
public interface IDifficultyLevelRepository : ICategoryRepository<DifficultyLevel, int>
{
    /// <summary>Tìm theo mã</summary>
    Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default);
}

/// <summary>Interface repository cho QuestionType</summary>
public interface IQuestionTypeRepository : ICategoryRepository<QuestionType, int>
{
    /// <summary>Tìm theo mã</summary>
    Task<QuestionType?> GetByCodeAsync(string code, CancellationToken ct = default);
}

