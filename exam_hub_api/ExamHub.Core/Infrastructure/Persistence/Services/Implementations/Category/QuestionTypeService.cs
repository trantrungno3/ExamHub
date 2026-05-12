using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

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

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
