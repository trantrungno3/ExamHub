using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho School</summary>
public interface ISchoolRepository : IBaseRepository<School, int>
{
    /// <summary>Lấy danh sách trường đang hoạt động</summary>
    Task<IReadOnlyList<School>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Tìm trường theo mã</summary>
    Task<School?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Lấy trường kèm danh sách khoá học</summary>
    Task<School?> GetWithCohortsAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy trường kèm danh sách thành viên (giáo viên/admin)</summary>
    Task<School?> GetWithMembersAsync(int id, CancellationToken ct = default);

    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}
