using System.Linq.Expressions;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeCohortClassRepository : ICohortClassRepository
{
    public List<CohortClass> Items { get; } = [];

    public Task<CohortClass?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClass>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClass>> GetAsync(Expression<Func<CohortClass, bool>> predicate, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortClass>>(Items.Where(predicate.Compile()).ToList());
    public Task<CohortClass?> FirstOrDefaultAsync(Expression<Func<CohortClass, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<CohortClass, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<CohortClass, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortClass> AddAsync(CohortClass entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<CohortClass> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(CohortClass entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(CohortClass entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClass>> GetByCohortAsync(int cohortId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClass>> GetBySchoolYearAsync(string schoolYear, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetHomeroomTeacherAsync(int id, Guid? teacherId, CancellationToken ct = default) => throw new NotSupportedException();
}

public class CohortClassServiceGetByHomeroomTeacherTests
{
    [Fact]
    public async Task GetByHomeroomTeacherAsync_ReturnsOnlyClassesWhereTeacherIsHomeroom()
    {
        var repo = new FakeCohortClassRepository();
        var teacherId = Guid.NewGuid();
        repo.Items.Add(new CohortClass { Id = 1, CohortId = 1, GradeLevelId = 1, ClassName = "10A", SchoolYear = "2025-2026", YearIndex = 1, HomeroomTeacherId = teacherId });
        repo.Items.Add(new CohortClass { Id = 2, CohortId = 1, GradeLevelId = 1, ClassName = "10B", SchoolYear = "2025-2026", YearIndex = 1, HomeroomTeacherId = Guid.NewGuid() });
        repo.Items.Add(new CohortClass { Id = 3, CohortId = 1, GradeLevelId = 1, ClassName = "10C", SchoolYear = "2025-2026", YearIndex = 1, HomeroomTeacherId = null });
        var service = new CohortClassService(repo);

        var result = await service.GetByHomeroomTeacherAsync(teacherId);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }
}
