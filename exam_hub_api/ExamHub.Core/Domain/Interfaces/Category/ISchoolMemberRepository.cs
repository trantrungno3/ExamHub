using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho SchoolMember</summary>
public interface ISchoolMemberRepository : IBaseRepository<SchoolMember, Guid>
{
    /// <summary>Lấy danh sách thành viên theo trường</summary>
    Task<IReadOnlyList<SchoolMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default);

    /// <summary>Lấy danh sách thành viên theo trường và vai trò</summary>
    Task<IReadOnlyList<SchoolMember>> GetBySchoolAndRoleAsync(int schoolId, string role, CancellationToken ct = default);

    /// <summary>Lấy tất cả trường mà một người dùng thuộc vào</summary>
    Task<IReadOnlyList<SchoolMember>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Lấy bản ghi theo trường + người dùng</summary>
    Task<SchoolMember?> GetBySchoolAndUserAsync(int schoolId, Guid userId, CancellationToken ct = default);

    /// <summary>Bật/tắt trạng thái thành viên</summary>
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
}
