using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using TVT.Core;

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

    public Task<IReadOnlyList<CohortMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => _repo.GetBySchoolAsync(schoolId, ct);

    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => _repo.GetByStudentAsync(studentId, ct);

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<RequestResponse<CohortMember>> AddStudentAsync(CohortMember entity, CancellationToken ct = default)
    {
        if (await _repo.ExistsActiveMembershipAsync(entity.CohortId, entity.StudentId, ct))
            return RequestResponse<CohortMember>.Error("Học sinh đã thuộc lớp khác trong khối này.");
        entity.Section = NormalizeSection(entity.Section);
        var sectionError = await ValidateSectionAsync(entity.CohortId, entity.Section, ct);
        if (sectionError is not null) return RequestResponse<CohortMember>.Error(sectionError);
        entity.Id       = Guid.NewGuid();
        entity.JoinedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        entity.Created  = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;
        var added = await _repo.AddAsync(entity, ct);
        return RequestResponse<CohortMember>.Success("Thêm học sinh thành công!", added, 1);
    }

    public Task RemoveStudentAsync(Guid id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);

    public async Task<RequestResponse<bool>> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
    {
        var member = await _repo.GetByIdAsync(id, ct);
        if (member is null) return RequestResponse<bool>.Error("Không tìm thấy học sinh trong khoá.");
        section = NormalizeSection(section);
        var sectionError = await ValidateSectionAsync(member.CohortId, section, ct);
        if (sectionError is not null) return RequestResponse<bool>.Error(sectionError);
        var updated = await _repo.SetSectionAsync(id, section, ct);
        return RequestResponse<bool>.Success("Cập nhật lớp thành công!", updated, 1);
    }

    // ── Helpers ─────────────────────────────────────────────────
    private static string? NormalizeSection(string? section)
        => string.IsNullOrWhiteSpace(section) ? null : section.Trim().ToUpperInvariant();

    /// <summary>Validates the section against the cohort's class range. Returns null when valid,
    /// or the Vietnamese error message when not.</summary>
    private async Task<string?> ValidateSectionAsync(int cohortId, string? section, CancellationToken ct)
    {
        if (section is null) return null; // chưa xếp lớp — hợp lệ
        var cohort = await _cohortRepo.GetByIdAsync(cohortId, ct);
        if (cohort is null) return "Không tìm thấy khoá học.";
        var allowed = Enumerable.Range(0, cohort.NumClasses)
            .Select(i => ((char)('A' + i)).ToString());
        return allowed.Contains(section)
            ? null
            : $"Lớp '{section}' không hợp lệ cho khoá này (chỉ A..{(char)('A' + cohort.NumClasses - 1)}).";
    }
}
