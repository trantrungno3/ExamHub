using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho SchoolMember</summary>
public interface ISchoolMemberService
{
    /// <summary>Lấy danh sách thành viên theo trường</summary>
    Task<IReadOnlyList<SchoolMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default);
    /// <summary>Lấy danh sách thành viên theo trường và vai trò</summary>
    Task<IReadOnlyList<SchoolMember>> GetBySchoolAndRoleAsync(int schoolId, string role, CancellationToken ct = default);
    /// <summary>Lấy tất cả trường mà một người dùng thuộc vào</summary>
    Task<IReadOnlyList<SchoolMember>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<SchoolMember?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Thêm thành viên vào trường</summary>
    Task<SchoolMember> AddMemberAsync(SchoolMember entity, CancellationToken ct = default);
    /// <summary>Cập nhật vai trò thành viên</summary>
    Task<SchoolMember> UpdateAsync(SchoolMember entity, CancellationToken ct = default);
    /// <summary>Xóa thành viên khỏi trường</summary>
    Task RemoveMemberAsync(Guid id, CancellationToken ct = default);
    /// <summary>Bật/tắt trạng thái thành viên</summary>
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
}
