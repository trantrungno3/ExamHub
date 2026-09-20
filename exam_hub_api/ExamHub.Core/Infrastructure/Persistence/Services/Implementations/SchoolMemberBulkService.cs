using ClosedXML.Excel;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Common;
using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TVT.Core.Enums;
using TVT.Core.IdentityUser.PostgreSql.Models;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

public sealed class SchoolMemberBulkService(
    IUserManagementService users,
    ISchoolMemberRepository schoolMembers,
    ICohortRepository cohorts,
    ICohortMemberRepository cohortMembers,
    ISchoolMemberService schoolMemberWriter,
    ICohortMemberService cohortMemberWriter,
    AppDbContext db) : ISchoolMemberBulkService
{
    private const int ColumnCount = 4;
    private const long MaxImportBytes = 10 * 1024 * 1024;
    private static readonly string[] Headers = ["UserName", "Role", "CohortName", "Section"];

    /// <summary>Vai trò hợp lệ trong file Excel.</summary>
    private static readonly string[] ImportRoles = ["Teacher", "Student"];

    /// <summary>Vai trò hợp lệ khi thêm thủ công; Admin chỉ dùng được ở đây.</summary>
    private static readonly string[] ManualRoles = ["Admin", "Teacher", "Student"];

    public async Task<SchoolMemberImportPreviewResponse> PreviewAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default)
    {
        var rows = await ValidateWorkbookAsync(request, ct);
        var results = rows
            .Select(row => new SchoolMemberImportRowResult(
                row.RowNumber, row.UserName, row.Role, row.CohortName, row.Section,
                row.Errors.Count == 0, row.Errors))
            .ToList();
        var validCount = results.Count(x => x.IsValid);
        return new(validCount, results.Count - validCount, results);
    }

    public async Task<SchoolMemberBulkResult> ImportAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default)
        => await WriteRowsAsync(await ValidateWorkbookAsync(request, ct), request.SchoolId, ct);

    public async Task<SchoolMemberBulkResult> AddAsync(
        SchoolMemberBulkAddRequest request, CancellationToken ct = default)
    {
        var context = await LoadContextAsync(request.SchoolId, ct);
        return await WriteRowsAsync(ValidateSelection(request, context), request.SchoolId, ct);
    }

    public byte[] BuildTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Members");
        for (var column = 0; column < Headers.Length; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = Headers[column];
            cell.Style.Font.Bold = true;
        }
        sheet.Cell(2, 1).Value = "teacher01";
        sheet.Cell(2, 2).Value = "Teacher";
        sheet.Cell(3, 1).Value = "student01";
        sheet.Cell(3, 2).Value = "Student";
        sheet.Cell(3, 3).Value = "Khoá 2026";
        sheet.Cell(3, 4).Value = "A";
        sheet.Columns().AdjustToContents();

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    // ── Writes ──────────────────────────────────────────────────
    /// <summary>Ghi từng dòng hợp lệ độc lập; một dòng lỗi không chặn các dòng sau.</summary>
    private async Task<SchoolMemberBulkResult> WriteRowsAsync(
        List<ValidatedRow> rows, int schoolId, CancellationToken ct)
    {
        var errors = new List<BulkImportRowError>();
        var successCount = 0;
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            if (row.Errors.Count > 0)
            {
                errors.Add(new(row.RowNumber, string.Join(" ", row.Errors)));
                continue;
            }

            try
            {
                await WriteAsync(row, schoolId, ct);
                successCount++;
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                errors.Add(new(row.RowNumber,
                    "Dữ liệu đã thay đổi; tài khoản có thể vừa được thêm vào trường."));
            }
            catch (InvalidOperationException ex)
            {
                db.ChangeTracker.Clear();
                errors.Add(new(row.RowNumber, ex.Message));
            }
        }

        return new(successCount, errors.Count, errors);
    }

    private async Task WriteAsync(ValidatedRow row, int schoolId, CancellationToken ct)
    {
        if (row.Role is "Admin" or "Teacher")
        {
            await schoolMemberWriter.AddMemberAsync(new SchoolMember
            {
                SchoolId = schoolId,
                UserId = row.User!.Id,
                Role = row.Role,
                IsActive = true,
            }, ct);
            return;
        }

        var response = await cohortMemberWriter.AddStudentAsync(new CohortMember
        {
            CohortId = row.Cohort!.Id,
            StudentId = row.User!.Id,
            Section = row.Section,
            IsActive = true,
        }, ct);
        if (response.Status == RequestResponseStatus.Error)
            throw new InvalidOperationException(response.Message);
    }

    // ── Excel validation ────────────────────────────────────────
    private async Task<List<ValidatedRow>> ValidateWorkbookAsync(
        SchoolMemberImportRequest request, CancellationToken ct)
    {
        ValidateFile(request.File);

        await using var stream = request.File.OpenReadStream();
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(stream);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("File Excel không hợp lệ.", ex);
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new ArgumentException("File Excel không có worksheet.");
            ValidateHeaders(sheet);

            var context = await LoadContextAsync(request.SchoolId, ct);
            var rows = new List<ValidatedRow>();
            var seenUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
            for (var row = 2; row <= lastRow; row++)
            {
                ct.ThrowIfCancellationRequested();
                if (RowIsEmpty(sheet, row)) continue;
                rows.Add(ValidateRow(sheet, row, seenUserNames, context));
            }

            return rows;
        }
    }

    private static ValidatedRow ValidateRow(
        IXLWorksheet sheet, int row, HashSet<string> seenUserNames, ValidationContext context)
    {
        var userName = Cell(sheet, row, 1);
        var rawRole = Cell(sheet, row, 2);
        var cohortName = NullIfEmpty(Cell(sheet, row, 3));
        var section = CohortMemberService.NormalizeSection(Cell(sheet, row, 4));
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(userName))
            errors.Add("Cột UserName không được để trống.");
        else if (!seenUserNames.Add(userName))
            errors.Add($"Tên đăng nhập '{userName}' bị trùng trong file.");

        var role = Canonical(ImportRoles, rawRole);
        if (string.IsNullOrWhiteSpace(rawRole))
            errors.Add("Cột Role không được để trống.");
        else if (role is null)
            errors.Add($"Vai trò '{rawRole}' không hợp lệ (chỉ Teacher/Student).");

        var user = string.IsNullOrWhiteSpace(userName)
            ? null
            : context.Users.FirstOrDefault(x => string.Equals(x.UserName, userName, StringComparison.OrdinalIgnoreCase));
        ValidateAccount(errors, user, userName, role);

        Cohort? cohort = null;
        if (role == "Teacher")
        {
            if (cohortName is not null || section is not null)
                errors.Add("Giáo viên không được có CohortName hoặc Section.");
        }
        else if (role == "Student")
        {
            if (cohortName is null) errors.Add("Cột CohortName không được để trống cho Student.");
            if (section is null) errors.Add("Cột Section không được để trống cho Student.");
            var matches = cohortName is null
                ? []
                : context.Cohorts
                    .Where(x => x.IsActive && string.Equals(x.Name.Trim(), cohortName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            if (matches.Count == 0 && cohortName is not null)
                errors.Add($"Không tìm thấy khoá học hoạt động '{cohortName}'.");
            if (matches.Count > 1)
                errors.Add($"Tên khoá học '{cohortName}' bị trùng.");
            cohort = matches.Count == 1 ? matches[0] : null;
            var sectionError = CohortMemberService.ValidateSection(cohort, section);
            if (sectionError is not null) errors.Add(sectionError);
        }

        ValidateMembership(errors, user, role, context);
        return new(row, userName, role ?? rawRole, cohortName, section, user, cohort, errors);
    }

    // ── Manual selection validation ─────────────────────────────
    /// <summary>Số dòng lỗi là vị trí 1-based trong danh sách người dùng đã chọn.</summary>
    private static List<ValidatedRow> ValidateSelection(
        SchoolMemberBulkAddRequest request, ValidationContext context)
    {
        var role = Canonical(ManualRoles, request.Role);
        var section = CohortMemberService.NormalizeSection(request.Section);
        var placement = new List<string>();
        Cohort? cohort = null;

        if (role is null)
            placement.Add($"Vai trò '{request.Role}' không hợp lệ (chỉ Admin/Teacher/Student).");
        else if (role == "Student")
        {
            if (request.CohortId is null) placement.Add("Vui lòng chọn khoá học.");
            else
            {
                cohort = context.Cohorts.FirstOrDefault(x => x.Id == request.CohortId);
                if (cohort is null) placement.Add("Không tìm thấy khoá học trong trường.");
                else if (!cohort.IsActive) placement.Add("Khoá học không hoạt động.");
            }
            if (section is null) placement.Add("Vui lòng chọn lớp.");
            else if (cohort is not null)
            {
                var sectionError = CohortMemberService.ValidateSection(cohort, section);
                if (sectionError is not null) placement.Add(sectionError);
            }
        }

        var rows = new List<ValidatedRow>();
        var seenUserIds = new HashSet<Guid>();
        for (var index = 0; index < request.UserIds.Count; index++)
        {
            var userId = request.UserIds[index];
            var errors = new List<string>(placement);
            if (!seenUserIds.Add(userId)) errors.Add("Tài khoản bị chọn trùng.");
            var user = context.Users.FirstOrDefault(x => x.Id == userId);
            ValidateAccount(errors, user, user?.UserName ?? userId.ToString(), role);
            ValidateMembership(errors, user, role, context);
            rows.Add(new(index + 1, user?.UserName ?? userId.ToString(), role ?? request.Role,
                cohort?.Name, section, user, cohort, errors));
        }

        return rows;
    }

    // ── Shared rules ────────────────────────────────────────────
    private static void ValidateAccount(
        List<string> errors, UserAdmin? user, string displayName, string? role)
    {
        if (user is null)
        {
            errors.Add($"Không tìm thấy tài khoản '{displayName}'.");
            return;
        }
        if (user.Deleted.HasValue)
        {
            errors.Add($"Tài khoản '{displayName}' đã bị xoá.");
            return;
        }
        if (role is not null && !user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Tài khoản '{displayName}' không có vai trò {role}.");
    }

    /// <summary>Thành viên đang hoạt động và ngừng hoạt động đều tính là đã tồn tại.</summary>
    private static void ValidateMembership(
        List<string> errors, UserAdmin? user, string? role, ValidationContext context)
    {
        if (user is null || role is null) return;
        if (role is "Admin" or "Teacher")
        {
            if (context.SchoolMembers.Any(x => x.UserId == user.Id))
                errors.Add($"Tài khoản '{user.UserName}' đã là thành viên của trường.");
        }
        else if (context.CohortMembers.Any(x => x.StudentId == user.Id))
            errors.Add($"Học sinh '{user.UserName}' đã thuộc một khoá của trường.");
    }

    private async Task<ValidationContext> LoadContextAsync(int schoolId, CancellationToken ct)
    {
        var schoolCohorts = await cohorts.GetBySchoolAsync(schoolId, ct);
        var cohortIds = schoolCohorts.Select(x => x.Id).ToHashSet();
        IReadOnlyList<CohortMember> existingCohortMembers = cohortIds.Count == 0
            ? []
            : await cohortMembers.GetAsync(x => cohortIds.Contains(x.CohortId), ct);
        return new(
            users.GetList().ToList(),
            await schoolMembers.GetAsync(x => x.SchoolId == schoolId, ct),
            schoolCohorts,
            existingCohortMembers);
    }

    private static string? Canonical(string[] allowed, string? value)
        => allowed.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));

    private static void ValidateHeaders(IXLWorksheet sheet)
    {
        if ((sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0) != Headers.Length)
            throw new ArgumentException("Tiêu đề Excel phải là UserName, Role, CohortName, Section.");
        for (var column = 0; column < Headers.Length; column++)
            if (!string.Equals(sheet.Cell(1, column + 1).GetString().Trim(), Headers[column], StringComparison.Ordinal))
                throw new ArgumentException("Tiêu đề Excel phải là UserName, Role, CohortName, Section.");
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidDataException("File import không được để trống.");
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Chỉ chấp nhận file Excel (.xlsx).");
        if (file.Length > MaxImportBytes)
            throw new InvalidDataException("File import không được vượt quá 10 MB.");
    }

    private static string Cell(IXLWorksheet sheet, int row, int column)
        => sheet.Cell(row, column).GetString().Trim();

    private static bool RowIsEmpty(IXLWorksheet sheet, int row)
        => Enumerable.Range(1, ColumnCount).All(column => string.IsNullOrWhiteSpace(Cell(sheet, row, column)));

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record ValidatedRow(
        int RowNumber, string UserName, string Role, string? CohortName, string? Section,
        UserAdmin? User, Cohort? Cohort, List<string> Errors);

    private sealed record ValidationContext(
        List<UserAdmin> Users,
        IReadOnlyList<SchoolMember> SchoolMembers,
        IReadOnlyList<Cohort> Cohorts,
        IReadOnlyList<CohortMember> CohortMembers);
}
