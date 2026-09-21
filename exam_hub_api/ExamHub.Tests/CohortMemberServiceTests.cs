using System.Linq.Expressions;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using TVT.Core.Enums;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeCohortMemberRepository : ICohortMemberRepository
{
    public List<CohortMember> Items { get; } = [];

    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember?> GetByCohortAndStudentAsync(int cohortId, Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
    {
        Items.First(x => x.Id == id).Section = section;
        return Task.FromResult(true);
    }

    public Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct = default)
        => Task.FromResult(Items.Any(x => x.CohortId == cohortId && x.StudentId == studentId && x.IsActive));

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<CohortMember>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember?> FirstOrDefaultAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<CohortMember, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<CohortMember> AddAsync(CohortMember entity, CancellationToken ct = default)
    {
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task AddRangeAsync(IEnumerable<CohortMember> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeCohortRepository : ICohortRepository
{
    public List<Cohort> Items { get; } = [];

    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> FirstOrDefaultAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<Cohort, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort> AddAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<Cohort> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

public class CohortMemberServiceTests
{
    [Fact]
    public async Task AddStudentAsync_AlreadyActiveMember_ReturnsError()
    {
        var repo = new FakeCohortMemberRepository();
        var studentId = Guid.NewGuid();
        repo.Items.Add(new CohortMember { Id = Guid.NewGuid(), CohortId = 1, StudentId = studentId, IsActive = true });
        var service = new CohortMemberService(repo, new FakeCohortRepository());

        var result = await service.AddStudentAsync(new CohortMember { CohortId = 1, StudentId = studentId });

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Học sinh đã thuộc lớp khác trong khối này.", result.Message);
    }

    [Fact]
    public async Task AddStudentAsync_SectionOutOfRange_ReturnsError()
    {
        var repo = new FakeCohortMemberRepository();
        var cohortRepo = new FakeCohortRepository();
        cohortRepo.Items.Add(new Cohort { Id = 1, Name = "K1", SchoolId = 1, NumClasses = 2 });
        var service = new CohortMemberService(repo, cohortRepo);

        var result = await service.AddStudentAsync(new CohortMember { CohortId = 1, StudentId = Guid.NewGuid(), Section = "C" });

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Lớp 'C' không hợp lệ cho khoá này (chỉ A..B).", result.Message);
    }

    [Fact]
    public async Task AddStudentAsync_ValidStudent_ReturnsSuccessAndAdds()
    {
        var repo = new FakeCohortMemberRepository();
        var cohortRepo = new FakeCohortRepository();
        cohortRepo.Items.Add(new Cohort { Id = 1, Name = "K1", SchoolId = 1, NumClasses = 2 });
        var service = new CohortMemberService(repo, cohortRepo);

        var result = await service.AddStudentAsync(new CohortMember { CohortId = 1, StudentId = Guid.NewGuid(), Section = "A" });

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Single(repo.Items);
    }

    [Fact]
    public async Task SetSectionAsync_MemberNotFound_ReturnsError()
    {
        var service = new CohortMemberService(new FakeCohortMemberRepository(), new FakeCohortRepository());

        var result = await service.SetSectionAsync(Guid.NewGuid(), "A");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Không tìm thấy học sinh trong khoá.", result.Message);
    }

    [Fact]
    public async Task SetSectionAsync_ValidSection_ReturnsSuccessAndUpdates()
    {
        var repo = new FakeCohortMemberRepository();
        var cohortRepo = new FakeCohortRepository();
        var memberId = Guid.NewGuid();
        repo.Items.Add(new CohortMember { Id = memberId, CohortId = 1, StudentId = Guid.NewGuid() });
        cohortRepo.Items.Add(new Cohort { Id = 1, Name = "K1", SchoolId = 1, NumClasses = 2 });
        var service = new CohortMemberService(repo, cohortRepo);

        var result = await service.SetSectionAsync(memberId, "b");

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal("B", repo.Items.Single().Section);
    }
}
