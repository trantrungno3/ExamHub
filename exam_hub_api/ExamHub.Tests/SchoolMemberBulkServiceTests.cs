using System.Linq.Expressions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TVT.Core.IdentityUser.PostgreSql.Models;

namespace ExamHub.Tests;

sealed class FakeBulkUsers : IUserManagementService
{
    public List<UserAdmin> Items { get; } = [];
    public IEnumerable<UserAdmin> GetList() => Items;
    public Task<UserAdmin?> FindByIdAsync(Guid id) => throw new NotSupportedException();
    public Task<bool> CheckUserNameExistAsync(string userName) => throw new NotSupportedException();
    public Task<bool> CheckUserExistByIdAsync(Guid id) => throw new NotSupportedException();
    public Task<UserAdmin?> CreateAsync(ExamHub.Core.DataTransferObjects.User.CreateUserRequest request) => throw new NotSupportedException();
    public Task<UserAdmin> UpdateAsync(UserAdmin user, ExamHub.Core.DataTransferObjects.User.UpdateUserRequest request) => throw new NotSupportedException();
    public Task DeleteAsync(UserAdmin user) => throw new NotSupportedException();
    public Task SetLockAsync(Guid id, bool isLocked) => throw new NotSupportedException();
    public Task ResetPasswordAsync(Guid id, string newPassword) => throw new NotSupportedException();
    public Task SetRolesAsync(Guid id, string[] roles) => throw new NotSupportedException();
    public Task<string[]?> AddRoleAsync(UserAdmin user, string role) => throw new NotSupportedException();
    public Task<string[]?> RemoveRoleAsync(UserAdmin user, string role) => throw new NotSupportedException();
}

sealed class FakeBulkSchoolMembers : ISchoolMemberRepository
{
    public List<SchoolMember> Items { get; } = [];
    public Task<IReadOnlyList<SchoolMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<SchoolMember>>(Items.Where(x => x.SchoolId == schoolId).ToList());
    public Task<IReadOnlyList<SchoolMember>> GetBySchoolAndRoleAsync(int schoolId, string role, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<SchoolMember>> GetByUserAsync(Guid userId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<SchoolMember?> GetBySchoolAndUserAsync(int schoolId, Guid userId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<SchoolMember?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<SchoolMember>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<SchoolMember>> GetAsync(Expression<Func<SchoolMember, bool>> predicate, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<SchoolMember>>(Items.Where(predicate.Compile()).ToList());
    public Task<SchoolMember?> FirstOrDefaultAsync(Expression<Func<SchoolMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<SchoolMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<SchoolMember, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<SchoolMember> AddAsync(SchoolMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<SchoolMember> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(SchoolMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(SchoolMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

sealed class FakeBulkCohorts : ICohortRepository
{
    public List<Cohort> Items { get; } = [];
    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Cohort>>(Items.Where(x => x.SchoolId == schoolId).ToList());
    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default) => throw new NotSupportedException();
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

sealed class FakeBulkCohortMembers : ICohortMemberRepository
{
    public List<CohortMember> Items { get; } = [];
    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<CohortMember>>(Items.Where(x => x.CohortId == cohortId).ToList());
    public Task<IReadOnlyList<CohortMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember?> GetByCohortAndStudentAsync(int cohortId, Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<CohortMember>>(Items.Where(predicate.Compile()).ToList());
    public Task<CohortMember?> FirstOrDefaultAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<CohortMember, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember> AddAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<CohortMember> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

sealed class SizedBulkFile(byte[] content, long length, string fileName) : IFormFile
{
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string ContentDisposition => "";
    public IHeaderDictionary Headers { get; } = new HeaderDictionary();
    public long Length => length;
    public string Name => "file";
    public string FileName => fileName;
    public void CopyTo(Stream target) => target.Write(content);
    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        => target.WriteAsync(content, cancellationToken).AsTask();
    public Stream OpenReadStream() => new MemoryStream(content, writable: false);
}

public class SchoolMemberBulkServicePreviewTests
{
    private const long MaxImportBytes = 10 * 1024 * 1024;

    [Theory]
    [InlineData("members.xlsx", 0, "File import không được để trống.")]
    [InlineData("members.csv", 1, "Chỉ chấp nhận file Excel (.xlsx).")]
    [InlineData("members.xlsx", MaxImportBytes + 1, "File import không được vượt quá 10 MB.")]
    public async Task PreviewAsync_InvalidFileBoundaryThrows(string fileName, long length, string message)
    {
        var fixture = Fixture.ValidSchool();
        var file = new FormFile(Stream.Null, 0, length, "file", fileName);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.Service.PreviewAsync(new(1, file)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task PreviewAsync_AcceptsUppercaseExtensionAtExactLimit()
    {
        var fixture = Fixture.ValidSchool();
        var source = Workbook(["teacher1", "Teacher", "", ""]);
        using var stream = source.OpenReadStream();
        using var output = new MemoryStream();
        stream.CopyTo(output);
        var file = new SizedBulkFile(output.ToArray(), MaxImportBytes, "MEMBERS.XLSX");

        var result = await fixture.Service.PreviewAsync(new(1, file));

        Assert.Equal(1, result.ValidCount);
    }

    [Fact]
    public async Task PreviewAsync_NormalizesValidTeacherAndStudentRows()
    {
        var fixture = Fixture.ValidSchool();
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(
            [" teacher1 ", "teacher", "", ""],
            ["student1", "STUDENT", " Khoá 2026 ", "b"])));

        Assert.Equal(2, result.ValidCount);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal("Teacher", result.Rows[0].Role);
        Assert.Equal("B", result.Rows[1].Section);
    }

    [Fact]
    public async Task PreviewAsync_LaterCaseInsensitiveDuplicateIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(
            ["teacher1", "Teacher", "", ""],
            [" TEACHER1 ", "Teacher", "", ""])));

        Assert.True(result.Rows[0].IsValid);
        Assert.False(result.Rows[1].IsValid);
        Assert.Contains("trùng trong file", result.Rows[1].Errors.Single());
    }

    [Fact]
    public async Task PreviewAsync_InactiveExistingTeacherIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        fixture.SchoolMembers.Items.Add(new() { SchoolId = 1, UserId = fixture.Teacher.Id, Role = "Teacher", IsActive = false });
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["teacher1", "Teacher", "", ""])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_InactiveStudentMembershipInAnotherSchoolCohortIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        fixture.Cohorts.Items.Add(new() { Id = 2, SchoolId = 1, Name = "Khoá 2027", NumClasses = 2 });
        fixture.CohortMembers.Items.Add(new() { CohortId = 2, StudentId = fixture.Student.Id, IsActive = false });
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["student1", "Student", "Khoá 2026", "A"])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_AmbiguousCohortNameIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        fixture.Cohorts.Items.Add(new() { Id = 2, SchoolId = 1, Name = " khoá 2026 ", NumClasses = 2 });
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["student1", "Student", "Khoá 2026", "A"])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_UnknownOrDeletedUserIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        fixture.Users.Items.Add(DeletedUser("deleted", "Teacher"));
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(
            ["missing", "Teacher", "", ""],
            ["deleted", "Teacher", "", ""])));
        Assert.All(result.Rows, row => Assert.False(row.IsValid));
    }

    [Fact]
    public async Task PreviewAsync_GlobalRoleMismatchIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["teacher1", "Student", "Khoá 2026", "A"])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_MissingStudentCohortOrSectionIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(
            ["student1", "Student", "", "A"],
            ["student2", "Student", "Khoá 2026", ""])));
        Assert.All(result.Rows, row => Assert.False(row.IsValid));
    }

    [Fact]
    public async Task PreviewAsync_InactiveCohortIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        fixture.Cohorts.Items.Single().IsActive = false;
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["student1", "Student", "Khoá 2026", "A"])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_InvalidSectionIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["student1", "Student", "Khoá 2026", "C"])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_TeacherWithCohortDataIsInvalid()
    {
        var fixture = Fixture.ValidSchool();
        var result = await fixture.Service.PreviewAsync(new(1, Workbook(["teacher1", "Teacher", "Khoá 2026", "A"])));
        Assert.False(result.Rows.Single().IsValid);
    }

    [Fact]
    public async Task PreviewAsync_WrongHeadersFails()
    {
        var fixture = Fixture.ValidSchool();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.PreviewAsync(new(1, WorkbookWithHeaders(["Role", "UserName", "CohortName", "Section"], ["teacher1", "Teacher", "", ""]))));
    }

    [Fact]
    public async Task PreviewAsync_ExtraHeaderFails()
    {
        var fixture = Fixture.ValidSchool();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.PreviewAsync(new(1, WorkbookWithHeaders(["UserName", "Role", "CohortName", "Section", "Extra"], ["teacher1", "Teacher", "", ""]))));
    }

    [Fact]
    public async Task PreviewAsync_WorkbookWithoutWorksheetsFails()
    {
        var fixture = Fixture.ValidSchool();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.PreviewAsync(new(1, EmptyWorkbook())));
    }

    private static FormFile Workbook(params string[][] rows)
        => WorkbookWithHeaders(["UserName", "Role", "CohortName", "Section"], rows);

    private static FormFile WorkbookWithHeaders(string[] headers, params string[][] rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Members");
        for (var column = 0; column < headers.Length; column++) sheet.Cell(1, column + 1).Value = headers[column];
        for (var row = 0; row < rows.Length; row++)
            for (var column = 0; column < rows[row].Length; column++) sheet.Cell(row + 2, column + 1).Value = rows[row][column];
        using var output = new MemoryStream();
        workbook.SaveAs(output);
        var bytes = output.ToArray();
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "members.xlsx");
    }

    private static FormFile EmptyWorkbook()
    {
        using var output = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(output, SpreadsheetDocumentType.Workbook, true))
        {
            var workbook = document.AddWorkbookPart();
            workbook.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
            workbook.Workbook.Save();
        }
        var bytes = output.ToArray();
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "members.xlsx");
    }

    private static UserAdmin DeletedUser(string userName, string role)
    {
        var user = User(userName, role);
        user.Deleted = DateTime.UtcNow;
        return user;
    }

    private static UserAdmin User(string userName, string role)
    {
        var user = new UserAdmin { Id = Guid.NewGuid(), UserName = userName, DisplayName = userName };
        user.AddRole(role);
        return user;
    }

    private sealed class Fixture
    {
        public required SchoolMemberBulkService Service { get; init; }
        public required FakeBulkUsers Users { get; init; }
        public required FakeBulkSchoolMembers SchoolMembers { get; init; }
        public required FakeBulkCohorts Cohorts { get; init; }
        public required FakeBulkCohortMembers CohortMembers { get; init; }
        public required UserAdmin Teacher { get; init; }
        public required UserAdmin Student { get; init; }

        public static Fixture ValidSchool()
        {
            var users = new FakeBulkUsers();
            var teacher = User("teacher1", "Teacher");
            var student = User("student1", "Student");
            users.Items.AddRange([teacher, student, User("student2", "Student")]);
            var schoolMembers = new FakeBulkSchoolMembers();
            var cohorts = new FakeBulkCohorts();
            cohorts.Items.Add(new() { Id = 1, SchoolId = 1, Name = "Khoá 2026", NumClasses = 2, IsActive = true });
            var cohortMembers = new FakeBulkCohortMembers();
            return new()
            {
                Users = users,
                SchoolMembers = schoolMembers,
                Cohorts = cohorts,
                CohortMembers = cohortMembers,
                Teacher = teacher,
                Student = student,
                Service = new SchoolMemberBulkService(users, schoolMembers, cohorts, cohortMembers, null!, null!,
                    new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options)),
            };
        }
    }
}
