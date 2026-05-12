using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho School</summary>
public interface ISchoolService : ICategoryService<School, int>
{
    /// <summary>Lấy theo mã trường</summary>
    Task<School?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Lấy trường kèm danh sách khoá học</summary>
    Task<School?> GetWithCohortsAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy trường kèm danh sách thành viên</summary>
    Task<School?> GetWithMembersAsync(int id, CancellationToken ct = default);
}
