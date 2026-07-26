using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho CohortMember</summary>
public class CohortMemberService : ICohortMemberService
{
    private readonly ICohortMemberRepository _repo;
    private readonly ICohortRepository _cohortRepo;

    public CohortMemberService(ICohortMemberRepository repo, ICohortRepository cohortRepo)
    {
        _repo = repo;
        _cohortRepo = cohortRepo;
    }

    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => _repo.GetByCohortAsync(cohortId, ct);

    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => _repo.GetByStudentAsync(studentId, ct);

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<CohortMember> AddStudentAsync(CohortMember entity, CancellationToken ct = default)
    {
        entity.Section = NormalizeSection(entity.Section);
        await ValidateSectionAsync(entity.CohortId, entity.Section, ct);
        entity.Id       = Guid.NewGuid();
        entity.JoinedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        entity.Created  = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public Task RemoveStudentAsync(Guid id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);

    public async Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
    {
        var member = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Không tìm thấy học sinh trong khoá.");
        section = NormalizeSection(section);
        await ValidateSectionAsync(member.CohortId, section, ct);
        return await _repo.SetSectionAsync(id, section, ct);
    }

    // ── Helpers ─────────────────────────────────────────────────
    private static string? NormalizeSection(string? section)
        => string.IsNullOrWhiteSpace(section) ? null : section.Trim().ToUpperInvariant();

    private async Task ValidateSectionAsync(int cohortId, string? section, CancellationToken ct)
    {
        if (section is null) return; // chưa xếp lớp — hợp lệ
        var cohort = await _cohortRepo.GetByIdAsync(cohortId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy khoá học.");
        var allowed = Enumerable.Range(0, cohort.NumClasses)
            .Select(i => ((char)('A' + i)).ToString());
        if (!allowed.Contains(section))
            throw new InvalidOperationException(
                $"Lớp '{section}' không hợp lệ cho khoá này (chỉ A..{(char)('A' + cohort.NumClasses - 1)}).");
    }
}
