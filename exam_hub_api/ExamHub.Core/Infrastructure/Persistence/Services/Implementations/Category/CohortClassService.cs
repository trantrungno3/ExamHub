using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho CohortClass (chủ yếu là đọc — DB trigger sinh tự động)</summary>
public class CohortClassService : ICohortClassService
{
    private readonly ICohortClassRepository _repo;
    public CohortClassService(ICohortClassRepository repo) => _repo = repo;

    public Task<IReadOnlyList<CohortClass>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => _repo.GetByCohortAsync(cohortId, ct);

    public Task<IReadOnlyList<CohortClass>> GetBySchoolYearAsync(string schoolYear, CancellationToken ct = default)
        => _repo.GetBySchoolYearAsync(schoolYear, ct);

    public Task<CohortClass?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<bool> SetHomeroomTeacherAsync(int id, Guid? teacherId, CancellationToken ct = default)
        => _repo.SetHomeroomTeacherAsync(id, teacherId, ct);
}
