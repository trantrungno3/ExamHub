using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho CohortClassTeacher (phân công GV giảng dạy cho lớp)</summary>
public class CohortClassTeacherService : ICohortClassTeacherService
{
    private readonly ICohortClassTeacherRepository _repo;
    public CohortClassTeacherService(ICohortClassTeacherRepository repo) => _repo = repo;

    /// <inheritdoc/>
    public Task<IReadOnlyList<CohortClassTeacher>> GetByClassAsync(int cohortClassId, CancellationToken ct = default)
        => _repo.GetAsync(x => x.CohortClassId == cohortClassId, ct);

    /// <inheritdoc/>
    public Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default)
        => _repo.GetEligibleTeacherIdsAsync(cohortClassId, subjectId, ct);

    /// <inheritdoc/>
    public async Task<CohortClassTeacher> AssignAsync(int cohortClassId, int subjectId, Guid teacherId, CancellationToken ct = default)
    {
        // 1) Validate dữ liệu đầu vào
        if (cohortClassId <= 0 || subjectId <= 0 || teacherId == Guid.Empty)
            throw new InvalidOperationException("Dữ liệu phân công không hợp lệ.");

        // 2) Ràng buộc: GV phải hợp lệ (thành viên trường role Teacher + dạy đúng môn)
        var eligible = await _repo.GetEligibleTeacherIdsAsync(cohortClassId, subjectId, ct);
        if (!eligible.Contains(teacherId))
            throw new InvalidOperationException("Giáo viên không hợp lệ cho môn học / trường này.");

        // 3) Ràng buộc: 1 môn/lớp = 1 GV
        var duplicated = await _repo.ExistsAsync(
            x => x.CohortClassId == cohortClassId && x.SubjectId == subjectId, ct);
        if (duplicated)
            throw new InvalidOperationException("Môn học đã được phân công cho giáo viên khác trong lớp này.");

        // 4) Hợp lệ → ghi DB
        return await _repo.AddAsync(
            new CohortClassTeacher { CohortClassId = cohortClassId, SubjectId = subjectId, TeacherId = teacherId }, ct);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);
}
