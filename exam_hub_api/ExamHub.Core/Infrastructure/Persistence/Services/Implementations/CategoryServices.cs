using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;


namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho GradeLevel</summary>
public class GradeLevelService : IGradeLevelService
{
    private readonly IGradeLevelRepository _repo;
    public GradeLevelService(IGradeLevelRepository repo) => _repo = repo;

    public Task<IReadOnlyList<GradeLevel>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<GradeLevel>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<GradeLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default)
        => _repo.GetWithSubjectsAsync(id, ct);

    public async Task<GradeLevel> CreateAsync(GradeLevel entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<GradeLevel> UpdateAsync(GradeLevel entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}

/// <summary>Triển khai service cho Subject</summary>
public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;
    public SubjectService(ISubjectRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<Subject>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => _repo.GetByGradeLevelAsync(gradeLevelId, ct);

    public Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default)
        => _repo.GetWithTopicsAsync(id, ct);

    public async Task<Subject> CreateAsync(Subject entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<Subject> UpdateAsync(Subject entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}

/// <summary>Triển khai service cho Topic</summary>
public class TopicService : ITopicService
{
    private readonly ITopicRepository _repo;
    public TopicService(ITopicRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => _repo.GetBySubjectAsync(subjectId, ct);

    public Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default)
        => _repo.GetRootTopicsAsync(subjectId, ct);

    public Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default)
        => _repo.GetChildrenAsync(parentId, ct);

    public Task<Topic?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<Topic> CreateAsync(Topic entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<Topic> UpdateAsync(Topic entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}

/// <summary>Triển khai service cho DifficultyLevel</summary>
public class DifficultyLevelService : IDifficultyLevelService
{
    private readonly IDifficultyLevelRepository _repo;
    public DifficultyLevelService(IDifficultyLevelRepository repo) => _repo = repo;

    public Task<IReadOnlyList<DifficultyLevel>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<DifficultyLevel>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<DifficultyLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public Task<DifficultyLevel> CreateAsync(DifficultyLevel entity, CancellationToken ct = default)
        => _repo.AddAsync(entity, ct);

    public async Task<DifficultyLevel> UpdateAsync(DifficultyLevel entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}

/// <summary>Triển khai service cho QuestionType</summary>
public class QuestionTypeService : IQuestionTypeService
{
    private readonly IQuestionTypeRepository _repo;
    public QuestionTypeService(IQuestionTypeRepository repo) => _repo = repo;

    public Task<IReadOnlyList<QuestionType>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<QuestionType>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<QuestionType?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<QuestionType?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public Task<QuestionType> CreateAsync(QuestionType entity, CancellationToken ct = default)
        => _repo.AddAsync(entity, ct);

    public async Task<QuestionType> UpdateAsync(QuestionType entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}

