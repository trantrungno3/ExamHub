using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho TeacherSubject</summary>
public class TeacherSubjectService : ITeacherSubjectService
{
    private readonly ITeacherSubjectRepository _repo;
    public TeacherSubjectService(ITeacherSubjectRepository repo) => _repo = repo;

    public Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default)
        => _repo.GetByTeacherAsync(userId, ct);

    public Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => _repo.IsTeacherOfSubjectAsync(userId, subjectId, ct);

    public Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => _repo.AssignSubjectAsync(userId, subjectId, ct);

    public Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => _repo.RemoveSubjectAsync(userId, subjectId, ct);
}
