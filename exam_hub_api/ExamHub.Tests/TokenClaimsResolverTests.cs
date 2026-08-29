using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeSchoolMemberService : ISchoolMemberService
{
    public List<SchoolMember> Members { get; } = [];
    public Task<IReadOnlyList<SchoolMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<SchoolMember>> GetBySchoolAndRoleAsync(int schoolId, string role, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<SchoolMember>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SchoolMember>>(Members.Where(x => x.UserId == userId).ToList());
    public Task<SchoolMember?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<SchoolMember> AddMemberAsync(SchoolMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<SchoolMember> UpdateAsync(SchoolMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task RemoveMemberAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeCohortClassTeacherService : ICohortClassTeacherService
{
    public List<CohortClassTeacher> Assignments { get; } = [];
    public Task<IReadOnlyList<CohortClassTeacher>> GetByClassAsync(int cohortClassId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortClassTeacher> AssignAsync(int cohortClassId, int subjectId, Guid teacherId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task RemoveAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClassTeacher>> GetByTeacherAsync(Guid teacherId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortClassTeacher>>(Assignments.Where(x => x.TeacherId == teacherId).ToList());
}

file sealed class FakeCohortClassService : ICohortClassService
{
    public List<CohortClass> Classes { get; } = [];
    public Task<IReadOnlyList<CohortClass>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortClass>>(Classes.Where(x => x.CohortId == cohortId).ToList());
    public Task<IReadOnlyList<CohortClass>> GetBySchoolYearAsync(string schoolYear, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortClass?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetHomeroomTeacherAsync(int id, Guid? teacherId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortClass>> GetByHomeroomTeacherAsync(Guid teacherId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortClass>>(Classes.Where(x => x.HomeroomTeacherId == teacherId).ToList());
}

file sealed class FakeTeacherSubjectService : ITeacherSubjectService
{
    public List<TeacherSubject> Subjects { get; } = [];
    public Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<TeacherSubject>>(Subjects.Where(x => x.UserId == userId).ToList());
    public Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeCohortMemberService : ICohortMemberService
{
    public List<CohortMember> Memberships { get; } = [];
    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CohortMember>>(Memberships.Where(x => x.StudentId == studentId).ToList());
    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember> AddStudentAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task RemoveStudentAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeCohortService : ICohortService
{
    public List<Cohort> Cohorts { get; } = [];
    public Task<IReadOnlyList<Cohort>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Cohorts.FirstOrDefault(x => x.Id == id));
    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort> CreateAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort> UpdateAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(int id, bool force, CancellationToken ct = default) => throw new NotSupportedException();
}

public class TokenClaimsResolverTests
{
    [Fact]
    public async Task ResolveAsync_Admin_ReturnsEmpty()
    {
        var resolver = new TokenClaimsResolver(
            new FakeSchoolMemberService(), new FakeCohortClassTeacherService(), new FakeCohortClassService(),
            new FakeTeacherSubjectService(), new FakeCohortMemberService(), new FakeCohortService());

        var result = await resolver.ResolveAsync(Guid.NewGuid(), ["Admin"]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_Teacher_ReturnsSchoolClassAndSubjectClaims_UnionOfTaughtAndHomeroom()
    {
        var schoolMembers = new FakeSchoolMemberService();
        var taught = new FakeCohortClassTeacherService();
        var classes = new FakeCohortClassService();
        var subjects = new FakeTeacherSubjectService();
        var resolver = new TokenClaimsResolver(
            schoolMembers, taught, classes, subjects, new FakeCohortMemberService(), new FakeCohortService());
        var teacherId = Guid.NewGuid();
        schoolMembers.Members.Add(new SchoolMember { SchoolId = 5, UserId = teacherId, Role = "Teacher", IsActive = true });
        taught.Assignments.Add(new CohortClassTeacher { Id = 1, CohortClassId = 100, SubjectId = 7, TeacherId = teacherId });
        classes.Classes.Add(new CohortClass { Id = 200, CohortId = 1, GradeLevelId = 1, ClassName = "10B", SchoolYear = "2025-2026", YearIndex = 1, HomeroomTeacherId = teacherId });
        subjects.Subjects.Add(new TeacherSubject { UserId = teacherId, SubjectId = 7 });
        subjects.Subjects.Add(new TeacherSubject { UserId = teacherId, SubjectId = 9 });

        var result = await resolver.ResolveAsync(teacherId, ["Teacher"]);

        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.SchoolId, "5"), result);
        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.CohortClassId, "100"), result);
        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.CohortClassId, "200"), result);
        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.SubjectId, "7"), result);
        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.SubjectId, "9"), result);
    }

    [Fact]
    public async Task ResolveAsync_Teacher_InactiveSchoolMembership_IsExcluded()
    {
        var schoolMembers = new FakeSchoolMemberService();
        var resolver = new TokenClaimsResolver(
            schoolMembers, new FakeCohortClassTeacherService(), new FakeCohortClassService(),
            new FakeTeacherSubjectService(), new FakeCohortMemberService(), new FakeCohortService());
        var teacherId = Guid.NewGuid();
        schoolMembers.Members.Add(new SchoolMember { SchoolId = 5, UserId = teacherId, Role = "Teacher", IsActive = false });

        var result = await resolver.ResolveAsync(teacherId, ["Teacher"]);

        Assert.DoesNotContain(result, x => x.Key == TokenClaimTypes.SchoolId);
    }

    [Fact]
    public async Task ResolveAsync_Student_WithSection_ResolvesToCurrentCohortClassId()
    {
        var classes = new FakeCohortClassService();
        var cohortMembers = new FakeCohortMemberService();
        var cohorts = new FakeCohortService();
        var resolver = new TokenClaimsResolver(
            new FakeSchoolMemberService(), new FakeCohortClassTeacherService(), classes,
            new FakeTeacherSubjectService(), cohortMembers, cohorts);
        var studentId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentYearIndex = SchoolYearCalculator.GetCurrentYearIndex(2023, today);
        cohorts.Cohorts.Add(new Cohort { Id = 1, SchoolId = 5, Name = "Khoá 2023", StartYear = 2023, EndYear = 2026, GradeStart = 10, NumClasses = 2 });
        cohortMembers.Memberships.Add(new CohortMember { CohortId = 1, StudentId = studentId, Section = "A", IsActive = true });
        classes.Classes.Add(new CohortClass { Id = 300, CohortId = 1, GradeLevelId = 1, ClassName = "X", SchoolYear = "irrelevant", YearIndex = (short)currentYearIndex, Section = "A" });
        classes.Classes.Add(new CohortClass { Id = 301, CohortId = 1, GradeLevelId = 1, ClassName = "X", SchoolYear = "irrelevant", YearIndex = (short)(currentYearIndex + 1), Section = "A" });

        var result = await resolver.ResolveAsync(studentId, ["Student"]);

        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.SchoolId, "5"), result);
        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.CohortClassId, "300"), result);
        Assert.DoesNotContain(new KeyValuePair<string, string>(TokenClaimTypes.CohortClassId, "301"), result);
    }

    [Fact]
    public async Task ResolveAsync_Student_NoSectionYet_OnlyGetsSchoolClaim()
    {
        var cohortMembers = new FakeCohortMemberService();
        var cohorts = new FakeCohortService();
        var resolver = new TokenClaimsResolver(
            new FakeSchoolMemberService(), new FakeCohortClassTeacherService(), new FakeCohortClassService(),
            new FakeTeacherSubjectService(), cohortMembers, cohorts);
        var studentId = Guid.NewGuid();
        cohorts.Cohorts.Add(new Cohort { Id = 1, SchoolId = 5, Name = "Khoá 2023", StartYear = 2023, EndYear = 2026, GradeStart = 10, NumClasses = 2 });
        cohortMembers.Memberships.Add(new CohortMember { CohortId = 1, StudentId = studentId, Section = null, IsActive = true });

        var result = await resolver.ResolveAsync(studentId, ["Student"]);

        Assert.Contains(new KeyValuePair<string, string>(TokenClaimTypes.SchoolId, "5"), result);
        Assert.DoesNotContain(result, x => x.Key == TokenClaimTypes.CohortClassId);
    }

    [Fact]
    public async Task ResolveAsync_Student_InactiveMembership_IsExcluded()
    {
        var cohortMembers = new FakeCohortMemberService();
        var cohorts = new FakeCohortService();
        var resolver = new TokenClaimsResolver(
            new FakeSchoolMemberService(), new FakeCohortClassTeacherService(), new FakeCohortClassService(),
            new FakeTeacherSubjectService(), cohortMembers, cohorts);
        var studentId = Guid.NewGuid();
        cohorts.Cohorts.Add(new Cohort { Id = 1, SchoolId = 5, Name = "Khoá 2023", StartYear = 2023, EndYear = 2026, GradeStart = 10, NumClasses = 2 });
        cohortMembers.Memberships.Add(new CohortMember { CohortId = 1, StudentId = studentId, Section = "A", IsActive = false });

        var result = await resolver.ResolveAsync(studentId, ["Student"]);

        Assert.Empty(result);
    }
}
