using System.Linq.Expressions;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeCohortClassTeacherRepository : ICohortClassTeacherRepository
{
    public List<CohortClassTeacher> Items { get; } = [];

    public Task<CohortClassTeacher?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClassTeacher>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClassTeacher>> GetAsync(Expression<Func<CohortClassTeacher, bool>> predicate, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortClassTeacher>>(Items.Where(predicate.Compile()).ToList());
    public Task<CohortClassTeacher?> FirstOrDefaultAsync(Expression<Func<CohortClassTeacher, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<CohortClassTeacher, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<CohortClassTeacher, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortClassTeacher> AddAsync(CohortClassTeacher entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<CohortClassTeacher> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(CohortClassTeacher entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(CohortClassTeacher entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
}

public class CohortClassTeacherServiceGetByTeacherTests
{
    [Fact]
    public async Task GetByTeacherAsync_ReturnsOnlyAssignmentsForThatTeacher()
    {
        var repo = new FakeCohortClassTeacherRepository();
        var teacherId = Guid.NewGuid();
        var otherTeacherId = Guid.NewGuid();
        repo.Items.Add(new CohortClassTeacher { Id = 1, CohortClassId = 10, SubjectId = 1, TeacherId = teacherId });
        repo.Items.Add(new CohortClassTeacher { Id = 2, CohortClassId = 11, SubjectId = 2, TeacherId = teacherId });
        repo.Items.Add(new CohortClassTeacher { Id = 3, CohortClassId = 12, SubjectId = 1, TeacherId = otherTeacherId });
        var service = new CohortClassTeacherService(repo);

        var result = await service.GetByTeacherAsync(teacherId);

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Equal(teacherId, x.TeacherId));
    }

    [Fact]
    public async Task GetByTeacherAsync_NoAssignments_ReturnsEmpty()
    {
        var repo = new FakeCohortClassTeacherRepository();
        var service = new CohortClassTeacherService(repo);

        var result = await service.GetByTeacherAsync(Guid.NewGuid());

        Assert.Empty(result);
    }
}
