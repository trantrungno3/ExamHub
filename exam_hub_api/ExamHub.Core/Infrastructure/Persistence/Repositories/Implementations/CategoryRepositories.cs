using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho GradeLevel</summary>
public class GradeLevelRepository : CategoryRepository<GradeLevel, int>, IGradeLevelRepository
{
    /// <inheritdoc/>
    public GradeLevelRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<GradeLevel>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.GradeNumber)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<GradeLevel>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(x => x.GradeNumber)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        return await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;
    }

    /// <inheritdoc/>
    public async Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Subjects)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}

/// <summary>Triển khai repository cho Subject</summary>
public class SubjectRepository : CategoryRepository<Subject, int>, ISubjectRepository
{
    /// <inheritdoc/>
    public SubjectRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Subject>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Subject>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.GradeLevelId == gradeLevelId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Topics)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}

/// <summary>Triển khai repository cho Topic</summary>
public class TopicRepository : CategoryRepository<Topic, int>, ITopicRepository
{
    /// <inheritdoc/>
    public TopicRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Topic>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Topic>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.ParentId == parentId && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.ParentId == null && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);
}

/// <summary>Triển khai repository cho DifficultyLevel</summary>
public class DifficultyLevelRepository : CategoryRepository<DifficultyLevel, int>, IDifficultyLevelRepository
{
    /// <inheritdoc/>
    public DifficultyLevelRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<DifficultyLevel>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<DifficultyLevel>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);
}

/// <summary>Triển khai repository cho QuestionType</summary>
public class QuestionTypeRepository : CategoryRepository<QuestionType, int>, IQuestionTypeRepository
{
    /// <inheritdoc/>
    public QuestionTypeRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<QuestionType>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<QuestionType>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<QuestionType?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);
}

