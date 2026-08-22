using System.Linq.Expressions;
using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Xunit;

namespace ExamHub.Tests;

// ── Hand-rolled fakes (no mocking library in this repo — xunit only) ──────

/// <summary>Fake ICohortMemberRepository backed by an in-memory list, with a shared call log.</summary>
file sealed class FakeCohortMemberRepository(List<string> callLog) : ICohortMemberRepository
{
    public List<CohortMember> Members { get; } = [];

    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortMember>>(Members.Where(m => m.CohortId == cohortId).ToList());

    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<CohortMember?> GetByCohortAndStudentAsync(int cohortId, Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<CohortMember>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<CohortMember>> GetAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<CohortMember?> FirstOrDefaultAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<int> CountAsync(Expression<Func<CohortMember, bool>>? predicate = null, CancellationToken ct = default)
    {
        var f = predicate?.Compile() ?? (_ => true);
        return Task.FromResult(Members.Count(f));
    }

    public Task<CohortMember> AddAsync(CohortMember entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task AddRangeAsync(IEnumerable<CohortMember> entities, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task UpdateAsync(CohortMember entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteAsync(CohortMember entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
    {
        Members.RemoveAll(m => m.Id == id);
        callLog.Add($"member-delete:{id}");
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

/// <summary>Fake ICohortRepository. DeleteByIdAsync throws by default — School's force-cascade
/// must go through ICohortService, never directly through this repo (see task-D4 fix ruling).
/// Pass allowDirectDelete:true only for the CohortService-level tests, where CohortService itself
/// legitimately deletes the cohort row via this repo as the final step.</summary>
file sealed class FakeCohortRepository(List<string> callLog, bool allowDirectDelete = false) : ICohortRepository
{
    public List<Cohort> Cohorts { get; } = [];

    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Cohort>>(Cohorts.Where(c => c.SchoolId == schoolId).ToList());

    public Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Cohort>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Cohort>> GetAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort?> FirstOrDefaultAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<int> CountAsync(Expression<Func<Cohort, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort> AddAsync(Cohort entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task AddRangeAsync(IEnumerable<Cohort> entities, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task UpdateAsync(Cohort entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteAsync(Cohort entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteByIdAsync(int id, CancellationToken ct = default)
    {
        if (!allowDirectDelete)
            throw new NotSupportedException(
                "FakeCohortRepository.DeleteByIdAsync should not be called directly during School force-cascade; " +
                "SchoolService must delegate to ICohortService.DeleteAsync(id, true, ct) instead.");
        Cohorts.RemoveAll(c => c.Id == id);
        callLog.Add($"cohort-delete:{id}");
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

/// <summary>Fake ISchoolRepository — only DeleteByIdAsync is exercised by SchoolService.DeleteAsync.</summary>
file sealed class FakeSchoolRepository(List<string> callLog) : ISchoolRepository
{
    public List<School> Schools { get; } = [];

    public Task<IReadOnlyList<School>> GetActiveAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<School?> GetByCodeAsync(string code, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<School?> GetWithCohortsAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<School?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<School?> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<School>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<School>> GetAsync(Expression<Func<School, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<School?> FirstOrDefaultAsync(Expression<Func<School, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(Expression<Func<School, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<int> CountAsync(Expression<Func<School, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<School> AddAsync(School entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task AddRangeAsync(IEnumerable<School> entities, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task UpdateAsync(School entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteAsync(School entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteByIdAsync(int id, CancellationToken ct = default)
    {
        Schools.RemoveAll(s => s.Id == id);
        callLog.Add($"school-delete:{id}");
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

/// <summary>Fake ICohortService — records every DeleteAsync(id, force, ct) call for the
/// SchoolService force-cascade assertions. All other members are unused by SchoolService.</summary>
file sealed class FakeCohortService(List<string> callLog) : ICohortService
{
    public List<(int Id, bool Force)> DeleteCalls { get; } = [];

    public Task DeleteAsync(int id, bool force, CancellationToken ct = default)
    {
        DeleteCalls.Add((id, force));
        callLog.Add($"cohortservice-delete:{id}:{force}");
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Cohort>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort> CreateAsync(Cohort entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Cohort> UpdateAsync(Cohort entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => throw new NotSupportedException();
}

// ── Tests ───────────────────────────────────────────────────────────────

public class CohortServiceDeleteTests
{
    private static Cohort NewCohort(int id, int schoolId = 1) => new()
    {
        Id = id,
        SchoolId = schoolId,
        Name = "K" + id,
        StartYear = 2020,
        EndYear = 2025,
        GradeStart = 10,
        NumClasses = 1
    };

    private static CohortMember NewMember(int cohortId) => new()
    {
        Id = Guid.NewGuid(),
        CohortId = cohortId,
        StudentId = Guid.NewGuid()
    };

    [Fact]
    public async Task DeleteAsync_NoForce_WithMembers_ThrowsEntityInUseException_AndDoesNotDeleteCohort()
    {
        var log = new List<string>();
        var memberRepo = new FakeCohortMemberRepository(log);
        var cohortRepo = new FakeCohortRepository(log, allowDirectDelete: true);
        cohortRepo.Cohorts.Add(NewCohort(1));
        memberRepo.Members.Add(NewMember(1));
        var service = new CohortService(cohortRepo, memberRepo);

        var ex = await Assert.ThrowsAsync<EntityInUseException>(() => service.DeleteAsync(1, force: false, ct: default));

        Assert.Equal("Khoá học còn học sinh/lớp đang hoạt động. Dùng xoá bắt buộc để xoá luôn dữ liệu liên quan.", ex.Message);
        Assert.DoesNotContain(log, e => e.StartsWith("cohort-delete:"));
        Assert.Single(cohortRepo.Cohorts); // parent untouched
    }

    [Fact]
    public async Task DeleteAsync_NoForce_NoMembers_DeletesCohortDirectly()
    {
        var log = new List<string>();
        var memberRepo = new FakeCohortMemberRepository(log);
        var cohortRepo = new FakeCohortRepository(log, allowDirectDelete: true);
        cohortRepo.Cohorts.Add(NewCohort(1));
        var service = new CohortService(cohortRepo, memberRepo);

        await service.DeleteAsync(1, force: false, ct: default);

        Assert.Empty(cohortRepo.Cohorts);
        Assert.Contains("cohort-delete:1", log);
    }

    [Fact]
    public async Task DeleteAsync_Force_WithMembers_DeletesMembersBeforeCohort()
    {
        var log = new List<string>();
        var memberRepo = new FakeCohortMemberRepository(log);
        var cohortRepo = new FakeCohortRepository(log, allowDirectDelete: true);
        cohortRepo.Cohorts.Add(NewCohort(1));
        var m1 = NewMember(1);
        var m2 = NewMember(1);
        memberRepo.Members.Add(m1);
        memberRepo.Members.Add(m2);
        var service = new CohortService(cohortRepo, memberRepo);

        await service.DeleteAsync(1, force: true, ct: default);

        Assert.Empty(memberRepo.Members);
        Assert.Empty(cohortRepo.Cohorts);
        Assert.Equal(
            [$"member-delete:{m1.Id}", $"member-delete:{m2.Id}", "cohort-delete:1"],
            log);
    }
}

public class SchoolServiceDeleteTests
{
    private static School NewSchool(int id) => new()
    {
        Id = id,
        Name = "S" + id,
        Code = "CODE" + id
    };

    private static Cohort NewCohort(int id, int schoolId) => new()
    {
        Id = id,
        SchoolId = schoolId,
        Name = "K" + id,
        StartYear = 2020,
        EndYear = 2025,
        GradeStart = 10,
        NumClasses = 1
    };

    [Fact]
    public async Task DeleteAsync_NoForce_WithCohorts_ThrowsEntityInUseException_AndDoesNotDeleteSchoolOrCascade()
    {
        var log = new List<string>();
        var schoolRepo = new FakeSchoolRepository(log);
        var cohortRepo = new FakeCohortRepository(log);
        var cohortService = new FakeCohortService(log);
        schoolRepo.Schools.Add(NewSchool(1));
        cohortRepo.Cohorts.Add(NewCohort(10, schoolId: 1));
        var service = new SchoolService(schoolRepo, cohortRepo, cohortService);

        var ex = await Assert.ThrowsAsync<EntityInUseException>(() => service.DeleteAsync(1, force: false, ct: default));

        Assert.Equal("Trường đang có khoá học/lớp liên kết. Dùng xoá bắt buộc để xoá luôn.", ex.Message);
        Assert.Single(schoolRepo.Schools); // parent untouched
        Assert.Empty(cohortService.DeleteCalls); // no cascade attempted
    }

    [Fact]
    public async Task DeleteAsync_NoForce_NoCohorts_DeletesSchoolDirectly()
    {
        var log = new List<string>();
        var schoolRepo = new FakeSchoolRepository(log);
        var cohortRepo = new FakeCohortRepository(log);
        var cohortService = new FakeCohortService(log);
        schoolRepo.Schools.Add(NewSchool(1));
        var service = new SchoolService(schoolRepo, cohortRepo, cohortService);

        await service.DeleteAsync(1, force: false, ct: default);

        Assert.Empty(schoolRepo.Schools);
        Assert.Contains("school-delete:1", log);
    }

    [Fact]
    public async Task DeleteAsync_Force_WithCohorts_CascadesThroughCohortService_ThenDeletesSchool()
    {
        var log = new List<string>();
        var schoolRepo = new FakeSchoolRepository(log);
        var cohortRepo = new FakeCohortRepository(log); // allowDirectDelete stays false: direct repo delete must NOT happen
        var cohortService = new FakeCohortService(log);
        schoolRepo.Schools.Add(NewSchool(1));
        cohortRepo.Cohorts.Add(NewCohort(10, schoolId: 1));
        cohortRepo.Cohorts.Add(NewCohort(11, schoolId: 1));
        var service = new SchoolService(schoolRepo, cohortRepo, cohortService);

        await service.DeleteAsync(1, force: true, ct: default);

        Assert.Empty(schoolRepo.Schools);
        Assert.Equal([(10, true), (11, true)], cohortService.DeleteCalls);
        Assert.Equal(
            ["cohortservice-delete:10:True", "cohortservice-delete:11:True", "school-delete:1"],
            log);
    }
}
