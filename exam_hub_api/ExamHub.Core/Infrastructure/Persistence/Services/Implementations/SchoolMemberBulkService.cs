using ClosedXML.Excel;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Common;
using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

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
    private static readonly string[] Headers = ["UserName", "Role", "CohortName", "Section"];
    private static readonly IReadOnlyDictionary<string, string> Roles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Teacher"] = "Teacher",
            ["Student"] = "Student",
        };

    public async Task<SchoolMemberImportPreviewResponse> PreviewAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(request.File);

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

        var schoolCohorts = await cohorts.GetBySchoolAsync(request.SchoolId, ct);
        var cohortIds = schoolCohorts.Select(x => x.Id).ToHashSet();
        var existingCohortMembers = cohortIds.Count == 0
            ? []
            : await cohortMembers.GetAsync(x => cohortIds.Contains(x.CohortId), ct);
        var context = new ValidationContext(
            users.GetList().ToList(),
            await schoolMembers.GetAsync(x => x.SchoolId == request.SchoolId, ct),
            schoolCohorts,
            existingCohortMembers);

        var rows = new List<SchoolMemberImportRowResult>();
        var seenUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        for (var row = 2; row <= lastRow; row++)
        {
            ct.ThrowIfCancellationRequested();
            if (RowIsEmpty(sheet, row)) continue;
            var validated = ValidateRow(sheet, row, seenUserNames, context);
            rows.Add(new(row, validated.UserName, validated.Role, validated.CohortName,
                validated.Section, validated.Errors.Count == 0, validated.Errors));
        }

        var validCount = rows.Count(x => x.IsValid);
        return new(validCount, rows.Count - validCount, rows);
        }
    }

    public Task<SchoolMemberBulkResult> ImportAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(schoolMemberWriter);
        throw new NotSupportedException();
    }

    public Task<SchoolMemberBulkResult> AddAsync(
        SchoolMemberBulkAddRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cohortMemberWriter);
        throw new NotSupportedException();
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

        var role = rawRole;
        if (string.IsNullOrWhiteSpace(rawRole))
            errors.Add("Cột Role không được để trống.");
        else if (!Roles.TryGetValue(rawRole, out role!))
            errors.Add($"Vai trò '{rawRole}' không hợp lệ (chỉ Teacher/Student).");

        var user = string.IsNullOrWhiteSpace(userName)
            ? null
            : context.Users.FirstOrDefault(x => string.Equals(x.UserName, userName, StringComparison.OrdinalIgnoreCase));
        if (user is null)
            errors.Add($"Không tìm thấy tài khoản '{userName}'.");
        else if (user.Deleted.HasValue)
            errors.Add($"Tài khoản '{userName}' đã bị xoá.");
        else if (role is "Teacher" or "Student" && !user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Tài khoản '{userName}' không có vai trò {role}.");

        Cohort? cohort = null;
        if (role == "Teacher")
        {
            if (cohortName is not null || section is not null)
                errors.Add("Giáo viên không được có CohortName hoặc Section.");
            if (user is not null && context.SchoolMembers.Any(x => x.UserId == user.Id))
                errors.Add($"Tài khoản '{userName}' đã là thành viên của trường.");
        }
        else if (role == "Student")
        {
            if (cohortName is null) errors.Add("Cột CohortName không được để trống cho Student.");
            if (section is null) errors.Add("Cột Section không được để trống cho Student.");
            var matches = cohortName is null
                ? []
                : context.Cohorts.Where(x => x.IsActive && string.Equals(x.Name.Trim(), cohortName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0 && cohortName is not null)
                errors.Add($"Không tìm thấy khoá học hoạt động '{cohortName}'.");
            if (matches.Count > 1)
                errors.Add($"Tên khoá học '{cohortName}' bị trùng.");
            cohort = matches.Count == 1 ? matches[0] : null;
            var sectionError = CohortMemberService.ValidateSection(cohort, section);
            if (sectionError is not null) errors.Add(sectionError);
            if (user is not null && context.CohortMembers.Any(x => x.StudentId == user.Id))
                errors.Add($"Học sinh '{userName}' đã thuộc một khoá của trường.");
        }

        return new(userName, role, cohortName, section, user, cohort, errors);
    }

    private static void ValidateHeaders(IXLWorksheet sheet)
    {
        if ((sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0) != Headers.Length)
            throw new ArgumentException("Tiêu đề Excel phải là UserName, Role, CohortName, Section.");
        for (var column = 0; column < Headers.Length; column++)
            if (!string.Equals(sheet.Cell(1, column + 1).GetString().Trim(), Headers[column], StringComparison.Ordinal))
                throw new ArgumentException("Tiêu đề Excel phải là UserName, Role, CohortName, Section.");
    }

    private static string Cell(IXLWorksheet sheet, int row, int column)
        => sheet.Cell(row, column).GetString().Trim();

    private static bool RowIsEmpty(IXLWorksheet sheet, int row)
        => Enumerable.Range(1, ColumnCount).All(column => string.IsNullOrWhiteSpace(Cell(sheet, row, column)));

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record ValidatedRow(
        string UserName, string Role, string? CohortName, string? Section,
        TVT.Core.IdentityUser.PostgreSql.Models.UserAdmin? User, Cohort? Cohort,
        List<string> Errors);

    private sealed record ValidationContext(
        List<TVT.Core.IdentityUser.PostgreSql.Models.UserAdmin> Users,
        IReadOnlyList<SchoolMember> SchoolMembers,
        IReadOnlyList<Cohort> Cohorts,
        IReadOnlyList<CohortMember> CohortMembers);
}
