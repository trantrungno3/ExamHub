using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Cohort</summary>
public class CohortService : ICohortService
{
    private readonly ICohortRepository _repo;
    private readonly ICohortMemberRepository _memberRepo;

    public CohortService(ICohortRepository repo, ICohortMemberRepository memberRepo)
    {
        _repo = repo;
        _memberRepo = memberRepo;
    }

    public Task<IReadOnlyList<Cohort>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => _repo.GetBySchoolAsync(schoolId, ct);

    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => _repo.ExistsAsync(e => e.Id == id, ct);

    public Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default)
        => _repo.GetWithClassesAsync(id, ct);

    public Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => _repo.GetWithMembersAsync(id, ct);

    public async Task<Cohort> CreateAsync(Cohort entity, CancellationToken ct = default)
    {
        entity.Created = DateTime.UtcNow;
        // DB trigger tự sinh cohort_classes sau khi INSERT
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<Cohort> UpdateAsync(Cohort entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => DeleteAsync(id, false, ct);

    public async Task DeleteAsync(int id, bool force, CancellationToken ct = default)
    {
        // Chỉ tính thành viên ĐANG HOẠT ĐỘNG — khớp đúng với thông báo bên dưới. Thành viên đã
        // rời lớp (IsActive = false) không chặn xoá; chúng bị dọn theo cascade như phía dưới.
        var hasActiveMembers = await _memberRepo.ExistsAsync(m => m.CohortId == id && m.IsActive, ct);
        if (hasActiveMembers && !force)
            throw new EntityInUseException("Khoá học còn học sinh/lớp đang hoạt động. Dùng xoá bắt buộc để xoá luôn dữ liệu liên quan.");

        // Không cần xoá thủ công cohort_members/cohort_classes: cả database_schema.sql
        // (cohort_id ... ON DELETE CASCADE) lẫn AppDbContext (OnDelete(DeleteBehavior.Cascade))
        // đều cascade khi xoá cohort, nên vòng lặp xoá từng thành viên chỉ là N round-trip thừa.
        await _repo.DeleteByIdAsync(id, ct);
    }

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
